using System;
using System.Linq;
using Distance.Runtime;

namespace Distance.Diagnostics.Arp
{
    /// <summary>
    /// Learns IP <-> MAC mapping from ARP packets.
    /// </summary>
    public class ArpMappingRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket arp = null;
            When()
                .Match(() => arp);
            Then()
                .Do(ctx => ctx.TryInsert(new ArpAddressMapping { EthAddr = arp.ArpSrcHw, IpAddr = arp.ArpSrcProtoIpv4 }));
        }
    }

    public class ArpRequestReplyRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket request = null;
            ArpPacket reply = null;

            When()
                .Match<ArpPacket>(() => request, x => x.Opcode == ArpOpcode.Request && !x.IsGratuitous)
                .Match<ArpPacket>(() => reply, x => x.Opcode == ArpOpcode.Reply, x => x.EthDst == request.EthSrc, x => x.FrameTimeRelative - request.FrameTimeRelative < 1);

            Then()
                .Do(ctx => ctx.TryInsert(new ArpRequestReply { Request = request, Reply = reply }));
        }
    }
    public class ArpNoReplyRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket request = null;
            When()
                .Match<ArpPacket>(() => request, x => x.Opcode == ArpOpcode.Request && !x.IsGratuitous)
                .Not<ArpPacket>(x => x.Opcode == ArpOpcode.Reply, x => x.EthDst == request.EthSrc, x => x.FrameTimeRelative - request.FrameTimeRelative < 1);

            Then()
                .Do(ctx => ctx.TryInsert(new ArpUnanswered { Request = request }));
        }
    }

    public class CheckPaddingRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket packet = null;
            When()
                .Match(() => packet, x => BadPadding(x));
            Then()
                .Do(ctx => ctx.Warn($"ARP packet {packet} has bad padding."));
        }

        private bool BadPadding(ArpPacket packet)
        {
            return packet.EthPadding.Any(x => !(x == '0' || x == ':'));
        }
    }
}
