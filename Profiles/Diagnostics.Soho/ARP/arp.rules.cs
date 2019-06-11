using Distance.Runtime;
using NRules.Fluent.Dsl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Diagnostics.Arp
{

    public enum ArpOpcode { Request = 1, Reply = 2};

    [Name("Arp.RequestResponse"), Description("The rule identifies pairs of request and response messages.")]
    public class ArpRequestReplyRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket request = null;
            ArpPacket reply = null;

            When()
                .Match<ArpPacket>(() => request, x => x.ArpOpcode == (int)ArpOpcode.Request)
                .Match<ArpPacket>(() => reply, x => x.ArpOpcode == (int)ArpOpcode.Reply, x => x.EthDst == request.EthSrc, x => x.FrameTimeRelative - request.FrameTimeRelative < 1);

            Then()
                .Do(ctx => ctx.TryInsert(new ArpRequestReply { Request = request, Reply = reply }));
        }
    }

    public class ArpMappingRule : DistanceRule
    {
        public override void Define()
        {
            ArpPacket reply = null;
            When()
                .Match<ArpPacket>(() => reply, x => x.ArpOpcode == (int)ArpOpcode.Reply);
            Then()
                .Do(ctx => ctx.TryInsert(new ArpAddressMapping {EthAddr = reply.ArpSrcHw, IpAddr = reply.ArpSrcProtoIpv4 }));
        }
    }
}
