namespace Distance.Diagnostics.Lan
{
    using Distance.Diagnostics.Arp;
    using Distance.Runtime;
    using Distance.Utils;
    using NRules.Fluent.Dsl;
    using NRules.RuleModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;


    public class IpPacketRule : DistanceRule
    {
        public override void Define()
        {
            IpPacket packet = null;

            When()
                .Match(() => packet);
            Then()
                .Do(ctx => InsertFacts(ctx, packet));
        }
        private void InsertFacts(IContext ctx, IpPacket packet)
        {
            ctx.TryInsert(new IpFlow { IpSrc = packet.IpSrc, IpDst = packet.IpDst });

            ctx.TryInsert(new IpSourceEndpoint { IpAddr = packet.IpSrc });
            ctx.TryInsert(new IpEndpoint { IpAddr = packet.IpSrc });
            ctx.TryInsert(new AddressMapping { IpAddr = packet.IpSrc, EthAddr = packet.EthSrc });

            ctx.TryInsert(new IpDestinationEndpoint { IpAddr = packet.IpDst });
            ctx.TryInsert(new IpEndpoint { IpAddr = packet.IpDst });
            ctx.TryInsert(new AddressMapping { IpAddr = packet.IpDst, EthAddr = packet.EthDst });

            if (packet.EthDst == "ff:ff:ff:ff:ff:ff" && packet.IpDst != "255.255.255.255")
            {
                var f = new LocalNetworkBroadcast { IpBroadcast = packet.IpDst };
                ctx.TryInsert(f);
            }
        }
    }

    /// <summary>
    /// Detect possible gateway endpoint in the network.
    /// </summary>
    /// <remarks>
    /// Gateway can be detected from IP packets. The gateway is used for communication outside the LAN, thus
    /// many external addresses are mapped to a single MAC address.
    /// </remarks>
    public class DetectGatewayRule : DistanceRule
    {
        public override void Define()
        {
            IGrouping<string,AddressMapping> group = null;
            ArpAddressMapping arpMapping = null;
            When()
                .Query(() => group, q =>
                    from m in q.Match<AddressMapping>()
                    group m by m.EthAddr into g
                    where g.Count() > 1
                    select g)
                .Match<ArpAddressMapping>(() => arpMapping, m => m.EthAddr == group.Key);
            Then()
                 .Do(ctx => EmitAndInfo(ctx, new GatewayCandidate { IpAddr = arpMapping.IpAddr, EthAddr = group.Key }));
        }

        private void EmitAndInfo(IContext ctx, GatewayCandidate gw)
        {
            ctx.Info($"gateway: ip.addr={gw.IpAddr} eth.addr={gw.EthAddr}");
            ctx.TryInsert(gw);
        }
    }

    /// <summary>
    /// Based on analysis of ARP communication we deduce LAN prefix.
    /// </summary>
    public class LocalPrefixRule : DistanceRule
    {
        public override void Define()
        {
            IEnumerable<string> localAddresses = null;
            IEnumerable<string> broadcastAddresses = null;
            When()
                .Query(() => localAddresses, q => q
                    .Match<ArpAddressMapping>()
                    .Select(x => x.IpAddr)
                    .Collect())
                .Query(() => broadcastAddresses, q => q
                    .Match<LocalNetworkBroadcast>()
                    .Select(x => x.IpBroadcast)
                    .Collect());

            Then()
                .Do(ctx => ComputePrefixes(ctx, localAddresses, broadcastAddresses));
        }

        private void ComputePrefixes(IContext ctx, IEnumerable<string> localAddresses, IEnumerable<string> broadcastAddresses)
        {
            if (localAddresses == null || localAddresses.Count() == 0) return;
            var addresses = broadcastAddresses!=null ? localAddresses.Union(broadcastAddresses): localAddresses;

            // compute wildcard:
            var wc = addresses.Select(s => (Address:IPAddress.Parse(s),Prefix:32)).Aggregate((x,y) => IPAddressUtils.CommonPrefix(x.Address, x.Prefix,y.Address, y.Prefix));
            ctx.Info($"Local prefix: {wc.Address}/{wc.Prefix}");
            ctx.TryInsert(new LocalNetworkPrefix { IpNetwork = wc.Address.ToString(), IpPrefix = wc.Prefix });
        }
    }

    /// <summary>
    /// Remote addresses are addresses observed in IP packets but not resolved locally.
    /// </summary>
    public class RemoteAddressesRule : DistanceRule
    {
        public override void Define()
        {
            LocalNetworkPrefix localNetworkPrefix = null;
            IGrouping<string, AddressMapping> group = null;

            When()
                .Match(() => localNetworkPrefix)
                .Query(() => group, q =>
                    from m in q.Match<AddressMapping>()
                    group m by LocalOrRemote(localNetworkPrefix, m) into g
                    select g
                );
            Then()
                .Do(ctx => ctx.Info($"{group.Key} hosts: {StringUtils.ToString(group.ToArray())}"));
        }
        string LocalOrRemote(LocalNetworkPrefix localNetworkPrefix, AddressMapping m)
        {
            return IPAddress.Parse(m.IpAddr).BelongsTo(IPAddress.Parse(localNetworkPrefix.IpNetwork), localNetworkPrefix.IpPrefix) ? "local" : "remote";
        }

    }

    public class DuplicateAddressRule : DistanceRule
    {
        public override void Define()
        {
            IGrouping<string, AddressMapping> group = null;
            When()
                .Query(() => group, q =>
                    from m in q.Match<AddressMapping>()
                    group m by m.IpAddr into g
                    where g.Count() > 1
                    select g);
            Then()
                 .Yield(_ => new IpAddressConflict { IpAddress = group.Key, EthAddresses = group.Select(x => x.EthAddr).ToArray() });
        }
    }

    //  
    /// <summary>
    /// A local host has an address that is not from LAN address scope. 
    /// </summary>
    /// <remarks>
    /// IP address outside the LAN - it behaves like local address but it is not:
    /// 
    /// </remarks>
    public class IpAddressMismatchRule : DistanceRule
    {
        public override void Define()
        {
            IpSourceEndpoint ipSrc = null;
            AddressMapping mapping = null;
            When()
                .Match(() => ipSrc)
                .Match(() => mapping, m => m.IpAddr == ipSrc.IpAddr)
                .Exists<ArpUnanswered>(x => x.Request.EthSrc == mapping.EthAddr)
                .Not<IpDestinationEndpoint>(d => ipSrc.IpAddr == d.IpAddr);
            Then()
                .Yield(_ => new IpAddressMismatch { IpAddress = ipSrc.IpAddr, EthAddress = mapping.EthAddr});
        }
    }

    /// <summary>
    /// The use of link local IPv4 address is detected by this rule.
    /// </summary>
    public class LinkLocalAddressUseRule : DistanceRule
    {
        IPAddress linkLocal = IPAddress.Parse("169.254.0.0");
        public override void Define()
        {
            IpSourceEndpoint ipSrc = null;
            AddressMapping mapping = null;
            When()
                .Match(() => ipSrc, x => IPAddress.Parse(x.IpAddr).BelongsTo(linkLocal, 16))
                .Match(() => mapping, m => m.IpAddr == ipSrc.IpAddr);
            Then()
                 .Yield(_ => new LinkLocalIpAddressUse { IpAddress = ipSrc.IpAddr, EthAddress = mapping.EthAddr });
        }
    }

    /// <summary>
    /// Can be detected by checking LAN broadcasts. It is ok, if there is only one LAN broadcasts.
    /// </summary>
    public class InvalidNetworkMaskRule : DistanceRule
    {
        public override void Define()
        {
            IEnumerable<BroadcastGroup> broadcasts = null;
            When()
                .Query(() => broadcasts, q => q
                    .Match<BroadcastGroup>()
                    .Collect());
            Then()
                .Do(ctx => Check(ctx, broadcasts.ToArray()));
        }

        private void Check(IContext ctx, BroadcastGroup[] broadcasts)
        {
            if (broadcasts.Count() > 1)
            {
                ctx.TryInsert(new MultipleBroadcastAddresses { Broadcasts = broadcasts });
            }
        }
    }

    public class CollectBroadcastersRule : DistanceRule
    { 
        public override void Define()
        {
            LocalNetworkBroadcast bcast = null; 
            IEnumerable<IpFlow> flows = null;
            When()
                .Match(() => bcast)
                .Query(() => flows, q =>
                    (from f in q.Match<IpFlow>()
                    where f.IpDst == bcast.IpBroadcast 
                    select f).Collect());
            Then()
                .Do(ctx => CheckNetworkMask(ctx, bcast, flows.ToArray()));
        }

        private void CheckNetworkMask(IContext ctx, LocalNetworkBroadcast bcast, IpFlow[] flows)
        {
                var bcasts = new BroadcastGroup
                {
                    IpBroadcast = bcast.IpBroadcast,
                    IpAddrs = flows.Select(x => x.IpSrc).ToArray()
                };
                ctx.TryInsert(bcasts);
        }
    }

    public class InvalidGatewayAddressRule : DistanceRule
    {
        IPAddress multicastIp = IPAddress.Parse("224.0.0.0");
        public override void Define()
        {
            LocalNetworkPrefix localPrefix = null;
            ArpUnanswered hostArp = null;
            When()
                .Match(() => localPrefix)
                .Match(() => hostArp)
                .All<IpFlow>(f => f.IpSrc == hostArp.Request.ArpSrcProtoIpv4 && IsLocalOrMulticast(f.IpDst, localPrefix));
            Then()
                .Do(ctx => CheckGw(ctx, hostArp));
        }
        bool IsLocalOrMulticast(string ipAddr, LocalNetworkPrefix localNetworkPrefix)
        {
            var ip = IPAddress.Parse(ipAddr);
            var isLocal =  ip.BelongsTo(IPAddress.Parse(localNetworkPrefix.IpNetwork), localNetworkPrefix.IpPrefix);
            var isMulticast = ip.BelongsTo(multicastIp, 3);
            return isLocal || isMulticast;
        }
        private void CheckGw(IContext ctx, ArpUnanswered badGwArp)
        {
            ctx.TryInsert(new InvalidGateway { HostIpAddr = badGwArp.Request.ArpSrcProtoIpv4, GwIpAddr = badGwArp.Request.ArpDstProtoIpv4 });    
        }
    }

}