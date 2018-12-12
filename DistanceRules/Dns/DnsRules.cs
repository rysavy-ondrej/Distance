using Distance.Domain.Dns;
using NRules.Fluent.Dsl;
using Distance.Utils;
using NRules.RuleModel;
using System;

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

    [Name("Dns.ResponseError"), Description("The rule is fired for all DNS responses with error code != 0.")]
    public class DnsResponseErrorRule : Rule
    {
        public override void Define()
        {
            DnsQueryResponseModel qr = null;
            When()
                .Match<DnsQueryResponseModel>(() => qr, x => x.Response.DnsFlagsRcode.ToInt(0) != 0);

            Then()
                .Do(ctx => ctx.Error($"DNS query {qr.Query} yields to error response {qr.Response}. Response time was {qr.Response.DnsTime}s."))
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
                .Match<DnsQueryResponseModel>(() => qr, x => x.Response.DnsTime.ToDouble(0) > 1.0);
            Then()
                .Do(ctx => ctx.Warn($"Response time is high ({qr.Response.DnsTime}s) for DNS query {qr.Query} and its response {qr.Response}."));
        }
    }
}
 