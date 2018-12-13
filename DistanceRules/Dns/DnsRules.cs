using Distance.Domain.Dns;
using NRules.Fluent.Dsl;
using Distance.Utils;
using NRules.RuleModel;
using System;
using System.Collections.Generic;

namespace Distance.Rules.Dns
{
    [Name("Dns.RequestResponse"), Description("The rule identifies pairs of request and response messages.")]
    public class DnsRequestResponseRule : Rule
    {
        public override void Define()
        {
            DnsModel query = null;
            DnsModel response = null;

            When()
                .Match<DnsModel>(() => query, x => x.DnsFlagsResponse == "0")
                .Match<DnsModel>(() => response, x => x.DnsFlagsResponse == "1", x => x.DnsId == query.DnsId);

            Then()
                .Yield(ctx => new DnsQueryResponseModel { Query = query, Response = response });
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
            DnsQueryResponseModel qr = null;
            When()
                .Match<DnsQueryResponseModel>(() => qr, x => x.Response.DnsFlagsRcode.ToInt(0) != 0);

            Then()
                .Do(ctx => ctx.Error($"DNS query {qr.Query} yields to error {(DnsResponseCode)qr.Response.DnsFlagsRcode.ToInt()} ({ResponseCodeDescription[(DnsResponseCode)qr.Response.DnsFlagsRcode.ToInt()]}) . DNS response {qr.Response}. Response time was {qr.Response.DnsTime}s."))
                .Yield(ctx => new DnsResponseErrorModel { Query = qr.Query, Response = qr.Response });
        }
    }


    [Name("Dns.NoResponse"), Description("The rule finds DNS requests without responses.")]
    public class NoResponseRule : Rule
    {
        public override void Define()
        {
            DnsModel query = null;
            When()
                .Match<DnsModel>(() => query, x => x.DnsFlagsResponse == "0")
                .Not<DnsModel>(x => x.DnsFlagsResponse == "1", x => x.DnsId == query.DnsId);

            Then()
                .Do(ctx => ctx.Error($"No Response for DNS query {query} found."))
                .Yield(_ => new DnsNoResponseModel { Query = query });
        }
    }

    [Name("Dns.DelayedResponse"), Description("The rule finds DNS replies that have latency greater than 1s.")]
    public class DelayedResponseRule : Rule
    {
        public override void Define()
        {
            DnsQueryResponseModel qr = null;
            When()
                .Match<DnsQueryResponseModel>(() => qr, x => x.Response.DnsTime.ToDouble() > 5.0);
            Then()
                .Do(ctx => ctx.Warn($"Response time is high ({qr.Response.DnsTime}s) for DNS query {qr.Query} and its response {qr.Response}."));
        }
    }
}
 