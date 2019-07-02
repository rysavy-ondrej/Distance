using System;
using System.Linq;
using Distance.Diagnostics.Lan;
using Distance.Runtime;
using Distance.Utils;
using NRules.Fluent.Dsl;
using NRules.RuleModel;

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
                //.Yield(_ => new ArpAddressMapping { EthAddr = arp.ArpSrcHwMac, IpAddr = arp.ArpSrcProtoIpv4 });
                .Do(ctx => InfoAndEmit(ctx, arp));
        }

        private void InfoAndEmit(IContext ctx, ArpPacket arp)
        {
            var mapping = new ArpAddressMapping { EthAddr = arp.ArpSrcHwMac, IpAddr =  arp.ArpSrcProtoIpv4 };
            ctx.Info($"ARP mapping: {mapping}");
            ctx.TryInsert(mapping);
        }
    }

    public class ArpRequestReplyRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket request = null;
            ArpPacket reply = null;

            When()
                .Match(() => request, x => x.Opcode == ArpOpcode.Request && !x.IsGratuitous)
                .Match(() => reply, x => x.Opcode == ArpOpcode.Reply, x => x.EthDst == request.EthSrc, x => x.FrameTimeRelative - request.FrameTimeRelative < 1);

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

    /// <summary>
    /// Tests if the ARP apcket has correct padding. If not, it generates warning to the log output.
    /// </summary>
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

    /// <summary>
    /// Finds gratuitous ARP with replies, which signalizes IP address conflicts.
    /// </summary>
    public class ArpReplyToGratuitousRequestRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket request = null;
            ArpPacket reply = null;

            When()
                .Match(() => request, x => x.Opcode == ArpOpcode.Request && x.IsGratuitous)
                .Match(() => reply, x => x.Opcode == ArpOpcode.Reply, x => x.EthDst == request.EthSrc, x => x.FrameTimeRelative - request.FrameTimeRelative < 1);

            Then()
                .Do(ctx => InfoAndEmit(ctx, request, reply)); //  
        }

        private void InfoAndEmit(IContext ctx, ArpPacket request, ArpPacket reply)
        {
            ctx.Warn($"Gratuitous ARP packet {request} has got answer {reply}. Possible problems in IP address assignement.");
            ctx.TryInsert(new ArpGratuitous { Request = request, Reply = reply });
        }
    }

    
    public class DetectArpPoissonRule : DistanceRule
    {
        public override void Define()
        {
            IGrouping<string, ArpAddressMapping> group = null;
            When()
                .Query(() => group, q => q
                    .Match<ArpAddressMapping>()
                    .GroupBy(m => m.IpAddr)
                 );
            Then()
                .Do(ctx => InfoAndEmit(ctx, group));
        }

        private void InfoAndEmit(IContext ctx, IGrouping<string, ArpAddressMapping> group)
        {
            var macs = group.Select(x => x.EthAddr).ToArray();
            ctx.Error($"Duplicate ARP address mapping detected: {group.Key} has resolutions to {StringUtils.ToString(macs)}. Bad IP address assignement or APR poissoning is in progress.");
            ctx.TryInsert(new IpAddressConflict { IpAddress = group.Key, EthAddresses = macs });
        }
    }
}
