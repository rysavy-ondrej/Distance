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
    /// Learns IP-to-MAC mapping from ARP packets.
    /// </summary>
    public class ArpMappingRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket arp = null;
            When()
                .Match(() => arp);
            Then()
                .Do(ctx => InfoAndEmit(ctx, arp));
        }

        private void InfoAndEmit(IContext ctx, ArpPacket arp)
        {
            var mapping = new ArpAddressMapping { EthAddr = arp.ArpSrcHwMac, IpAddr =  arp.ArpSrcProtoIpv4 };
            ctx.Info($"ARP mapping: {mapping}");
            ctx.TryInsert(mapping);
        }
    }

    /// <summary>
    /// Pairs ARP requests with corresponding ARP replies.
    /// </summary>
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
    /// <summary>
    /// Identifies ARP requests without adequate ARP replies.
    /// </summary>
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


    /// <summary>
    /// Detects ARP poison. This situation is characterized by the existence of multiple ARP replies from different hosts for the single ARP request.
    /// </summary>
    public class DetectArpPoisonRule : DistanceRule
    {
        public override void Define()
        {
            IGrouping<string, ArpAddressMapping> group = null;
            When()
                .Query(() => group, q => q
                    .Match<ArpAddressMapping>()
                    .GroupBy(m => m.IpAddr)
                    .Where(m => m.Count() > 1)
                 );
            Then()
                .Do(ctx => InfoAndEmit(ctx, group));
        }

        private void InfoAndEmit(IContext ctx, IGrouping<string, ArpAddressMapping> group)
        {
            var macs = group.Select(x => x.EthAddr).ToArray();
            ctx.Error($"Duplicate ARP address mapping detected: {group.Key} resolved to {StringUtils.ToString(macs)}. Bad IP address assignement or APR poissoning is in progress.");
            ctx.TryInsert(new ArpAddressConflict { IpAddress = group.Key, EthAddresses = macs });
        }
    }

    /// <summary>
    /// Detects ARP sweep activity. It amounts to track the number of ARP requests sent to different local addresses within the specified period of time.
    /// </summary>
    public class DetectArpSweepRule : DistanceRule
    {
        const int requestsThreshold = 30;
        const double timeIntervalLimit = 0.5;
        public override void Define()
        {
            IGrouping<string, ArpPacket> group = null;
            When()
                .Query(() => group, q => q
                    .Match<ArpPacket>(p => p.Opcode == ArpOpcode.Request)
                    .GroupBy(p => p.ArpSrcProtoIpv4)
                );
            Then()
                .Do(ctx => TestAndEmit(ctx, group));
        }


        /// <summary>
        /// Tests if ARP rerquests may represent an ARP sweep operation.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="group"></param>
        /// <remarks>
        /// ARP sweep is characterizes as sending more than X ARP requests to different addresses
        /// within Y seconds. 
        /// </remarks>
        private void TestAndEmit(IContext ctx, IGrouping<string, ArpPacket> group)
        {
            // the solution here is simple: group packets to windows of the specified lenght and compute 
            // if their number exceeds the given threshold
            var windows = group.GroupBy(x => (int)Math.Floor(x.FrameTimeRelative / timeIntervalLimit));
            foreach(var sweep in windows.Where(x => x.Count() > requestsThreshold))
            {
                ctx.Warn($"ARP sweep detected: {group.Key} sends {sweep.Count()} requests within {timeIntervalLimit} seconds.");
                ctx.TryInsert(new ArpSweepAttempt { IpAddress = group.Key, IpTargets = sweep.Select(x=>x.ArpDstProtoIpv4).ToArray() });
            }
        }
    }
}
