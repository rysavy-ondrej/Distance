using Distance.Rules;
using Distance.Utils;
using NRules.Fluent.Dsl;

namespace Distance.Rules.Icmp
{
    public class IcmpPacket
    {
        public static string Filter = "icmp";
        public static string[] Fields = { "frame.number", "ip.src", "ip.dst", "icmp.type", "icmp.code", "icmp.ident", "icmp.seq" };

        [FieldName("frame.number")]
        public int FrameNumber { get; set; }

        [FieldName("ip.src")]
        public string IpSrc { get; set; }

        [FieldName("ip.dst")]
        public string IpDst { get; set; }

        [FieldName("icmp.type")]
        public int IcmpType { get; set; }

        [FieldName("icmp.code")]
        public int IcmpCode { get; set; }

        [FieldName("icmp.ident")]
        public int IcmpIdent { get; set; }

        [FieldName("icmp.seq")]
        public int IcmpSeq { get; set; }

        public static IcmpPacket Create(string[] values)
        {
            return new IcmpPacket
            {
                FrameNumber = values[0].ToInt(),
                IpSrc = values[1].ToString(),
                IpDst = values[2].ToString(),
                IcmpType = values[3].ToInt(),
                IcmpCode = values[4].ToInt(),
                IcmpIdent = values[5].ToInt(),
                IcmpSeq = values[6].ToInt()
            };
        }
    }
    public class IcmpDestinationUnreachable
    {
        public IcmpPacket Packet { get; set; }
        public int Code { get; set; }
    }

    public class IcmpTtlExpired
    {
        public IcmpPacket Packet { get; set; }
    }


    public class TtlExpiredRule : Rule
    {
        public override void Define()
        {
            IcmpPacket packet = null;
            When()
                .Match(() => packet, p => p.IcmpType == 11);

            Then()
                .Yield(ctx => new IcmpTtlExpired { Packet = packet });
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
            IcmpTtlExpired expired = null;
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
