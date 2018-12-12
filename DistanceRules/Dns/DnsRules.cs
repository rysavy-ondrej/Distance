using Distance.Domain.Dns;
using NRules.Fluent.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
namespace Distance.Rules.Dns
{
    [Name("Dns.MatchRequestResponse"), Description("The rule identifies pairs of request and response messages.")]
    public class MatchRequestResponseRule : Rule
    {
        public override void Define()
        {
            DnsModel qry = null;
            DnsModel res = null;

            When()
                .Match<DnsModel>(() => qry, x => x.DnsFlagsResponse == "0")
                .Match<DnsModel>(() => res, x => x.DnsFlagsResponse == "1", x => x.DnsId == qry.DnsId);

            Then()
                .Do(ctx => ctx.Insert(new DnsQueryResponse { Query = qry, Response = res }));
        }
    }

    [Name("Dns.NoReply"), Description("The rule finds DNS requests without responses.")]
    public class NoReplyRule : Rule
    {
        public override void Define()
        {
            DnsModel qry = null;
            When()
                .Match<DnsModel>(() => qry, x => x.DnsFlagsResponse == "0")
                .Not<DnsModel>(x => x.DnsFlagsResponse == "1", x => x.DnsId == qry.DnsId);

            Then()
                .Do(ctx => ctx.Error($"No Response for DNS query {qry} found."));
        }
    }


    [Name("Dns.QueryResponse.Info"), Description("The rule prints information about every found pair of DNS messages.")]
    public class QueryResponseInfoRule : Rule
    {
        public override void Define()
        {
            DnsQueryResponse qr = null;
            When()
                .Match<DnsQueryResponse>(() => qr, x => x.Response.DnsTime.ToDouble(0) <= 1.0);
            Then()
                .Do(ctx => ctx.Info($"Detected DNS query {qr.Query} and its response {qr.Response}. Response time was {qr.Response.DnsTime}s."));
        }
    }

    [Name("Dns.ReplyBigLatency"), Description("The rule finds DNS replies that have latency greater than 1s.")]
    public class ReplyBigLatencyRule : Rule
    {
        public override void Define()
        {
            DnsQueryResponse qr = null;
            When()
                .Match<DnsQueryResponse>(() => qr, x => x.Response.DnsTime.ToDouble(0) > 1.0);
            Then()
                .Do(ctx => ctx.Warn($"Response time is high ({qr.Response.DnsTime}s) for DNS query {qr.Query} and its response {qr.Response}."));
        }
    }
}