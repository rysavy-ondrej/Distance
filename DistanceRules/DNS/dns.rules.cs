using Distance.Runtime;
using Distance.Utils;
using NRules.Fluent.Dsl;
using System.Collections.Generic;

namespace Distance.Diagnostics.Dns
{

    public class CollectDnsServerRule : Rule
    {
        public override void Define()
        {
            DnsPacket query = null;
            When()
                .Match(() => query, x => x.DnsFlagsResponse == false);
            Then()                    
                .Do(ctx  => ctx.TryInsert(new DnsServer { IpAddress = query.IpDst }));
        }
    }

    [Name("Dns.RequestResponse"), Description("The rule identifies pairs of request and response messages.")]
    public class DnsRequestResponseRule : Rule
    {
        public override void Define()
        {
            DnsPacket query = null;
            DnsPacket response = null;

            When()
                .Match<DnsPacket>(() => query, x => x.DnsFlagsResponse == false)
                .Match<DnsPacket>(() => response, x => x.DnsFlagsResponse == true, x => x.DnsId == query.DnsId);

            Then()
                .Do(ctx => ctx.TryInsert(new QueryResponse { Query = query, Response = response }));
        }
    }


    enum DnsResponseCode
    {
        NOERROR = 0,  // DNS Query completed successfully
        FORMERR = 1,  // DNS Query Format Error
        SERVFAIL = 2, // Server failed to complete the DNS request
        NXDOMAIN = 3, // Domain name does not exist.  
        NOTIMP = 4,   // Function not implemented
        REFUSED = 5,  // The server refused to answer for the query
        YXDOMAIN = 6, // Name that should not exist, does exist
        XRRSET = 7,   // RRset that should not exist, does exist
        NOTAUTH = 8,  // Server not authoritative for the zone
        NOTZONE = 9   // Name not in zone
    }
    
    

    [Name("Dns.ResponseError"), Description("The rule is fired for all DNS responses with error code != 0.")]
    public class DnsResponseErrorRule : Rule
    {
        static IDictionary<DnsResponseCode, string> ResponseCodeDescription = new Dictionary<DnsResponseCode, string>
        {
            [DnsResponseCode.NOERROR] = "DNS Query completed successfully",
            [DnsResponseCode.FORMERR] = "DNS Query Format Error",
            [DnsResponseCode.SERVFAIL] = "Server failed to complete the DNS request",
            [DnsResponseCode.NXDOMAIN] = "Domain name does not exist",
            [DnsResponseCode.NOTIMP] = "Function not implemented",
            [DnsResponseCode.REFUSED] = "The server refused to answer for the query",
            [DnsResponseCode.YXDOMAIN] = "Name that should not exist, does exist",
            [DnsResponseCode.XRRSET] = "RRset that should not exist, does exist",
            [DnsResponseCode.NOTAUTH] = "Server not authoritative for the zone",
            [DnsResponseCode.NOTZONE] = "Name not in zone",
        };

        public override void Define()
        {
            QueryResponse qr = null;
            When()
                .Match<QueryResponse>(() => qr, x => x.Response.DnsFlagsRcode != 0);

            Then()
                .Do(ctx => ctx.Error($"DNS query {qr.Query} yields to error {(DnsResponseCode)qr.Response.DnsFlagsRcode} ({ResponseCodeDescription[(DnsResponseCode)qr.Response.DnsFlagsRcode]}) . DNS response {qr.Response}. Response time was {qr.Response.DnsTime}s."))
                .Yield(ctx => new ResponseError { Query = qr.Query, Response = qr.Response });
        }
    }


    [Name("Dns.NoResponse"), Description("The rule finds DNS requests without responses.")]
    public class NoResponseRule : Rule
    {
        public override void Define()
        {
            DnsPacket query = null;
            When()
                .Match<DnsPacket>(() => query, x => x.DnsFlagsResponse == false)
                .Not<DnsPacket>(x => x.DnsFlagsResponse == true, x => x.DnsId == query.DnsId);

            Then()
                .Do(ctx => ctx.Error($"No Response for DNS query {query} found."))
                .Yield(_ => new NoResponse { Query = query });
        }
    }

    [Name("Dns.DelayedResponse"), Description("The rule finds DNS replies that have latency greater than 1s.")]
    public class DelayedResponseRule : Rule
    {
        public override void Define()
        {
            QueryResponse qr = null;
            When()
                .Match(() => qr, x => x.Response.DnsTime > 5.0);
            Then()
                .Do(ctx => ctx.Warn($"Response time is high ({qr.Response.DnsTime}s) for DNS query {qr.Query} and its response {qr.Response}."))
                .Yield(_ => new LateResponse { Query = qr.Query, Response=qr.Response, Delay = qr.Response.DnsTime });
        }
    }
    
    public class ServerUnresponsive : Rule
    {
        public override void Define()
        {
            DnsServer server = null;
            When()
                .Match(() => server);
      //          .Not<DnsQueryResponse>(other => server.IpAddress == other.Query.IpDst);
            Then()
                .Do(ctx => ctx.Error($"Dns server {server.IpAddress} does not response to any query. Either the DNS server {server.IpAddress} does not exists or it is down."));
        }
    } 
}
 