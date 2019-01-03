using Distance.Runtime;
using NRules.Fluent.Dsl;

namespace Distance.Diagnostics.Icmp
{
    public class TtlExpiredRule : Rule
    {
        public override void Define()
        {
            IcmpPacket packet = null;
            When()
                .Match(() => packet, p => p.IcmpType == 11);

            Then()
                .Yield(ctx => new TtlExpired { Packet = packet });
        }
    }

    public class DestinationUnreachableRule : Rule
    {
        public override void Define()
        {
            IcmpPacket packet = null;
            When()
                .Match(() => packet, p => p.IcmpType == 3);

            Then()
                .Yield(ctx => new IcmpDestinationUnreachable { Packet = packet, Code = packet.IcmpCode });
        }
    }

    public class TtlExpiredErrorRule : Rule
    {
        public override void Define()
        {
            TtlExpired expired = null;
            When()
                 .Match(() => expired);
            Then()
                .Do(ctx => ctx.Error($"TTL exceeded during connecting to ?. Message was sent from {expired.Packet.IpSrc}."));
        }
    }

    public class NetworkUnreachableErrorRule : Rule
    {
        public override void Define()
        {
            IcmpDestinationUnreachable unreachable = null;
            When()
                .Match(() => unreachable, x=>x.Code == 0);
            Then()
                .Do(ctx => ctx.Error($"Destination network unreachable while connecting to ?. Message was sent from {unreachable.Packet.IpSrc}."));
        }
    }
    public class HostUnreachableErrorRule : Rule
    {
        public override void Define()
        {
            IcmpDestinationUnreachable unreachable = null;
            When()
                .Match(() => unreachable, x => x.Code == 1);
            Then()
                .Do(ctx => ctx.Error($"Destination host unreachable while connecting to ?. Message was sent from {unreachable.Packet.IpSrc}."));
        }
    }
    public class PortUnreachableErrorRule : Rule
    {
        public override void Define()
        {
            IcmpDestinationUnreachable unreachable = null;
            When()
                .Match(() => unreachable, x => x.Code == 3);
            Then()
                .Do(ctx => ctx.Error($"Port unreachable while connecting to ?. Message was sent from {unreachable.Packet.IpSrc}."));
        }
    }
}
