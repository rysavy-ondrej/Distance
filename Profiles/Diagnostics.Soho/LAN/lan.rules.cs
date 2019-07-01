namespace Distance.Diagnostics.Lan
{
    using Distance.Diagnostics.Arp;
    using Distance.Runtime;
    using Distance.Utils;
    using NRules.Fluent.Dsl;
    using NRules.RuleModel;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;


    public enum AddressScope { LinkLocalAddress, LocalAddress, RemoteAddress, MulticastAddress }
    public static class LanRulesHelper
    {
        static IPAddress linkLocalIp = IPAddress.Parse("169.254.0.0");
        static IPAddress multicastIp = IPAddress.Parse("224.0.0.0");

        public static AddressScope GetAddressScope(string ipAddr, LocalNetworkPrefix localNetworkPrefix)
        {
            var ip = IPAddress.Parse(ipAddr);
            if (ip.BelongsTo(linkLocalIp, 16)) return AddressScope.LinkLocalAddress;
            if (ip.BelongsTo(multicastIp, 3)) return AddressScope.MulticastAddress;
            if (ip.BelongsTo(IPAddress.Parse(localNetworkPrefix.IpNetwork), localNetworkPrefix.IpPrefix)) return AddressScope.LocalAddress;
            else return AddressScope.RemoteAddress;
        }

        public static bool IsLinkLocal(string ipAddr)
        {
            var ip = IPAddress.Parse(ipAddr);
            return (ip.BelongsTo(linkLocalIp, 16));
        }
    }
    /// <summary>
    /// The rule matches all ip packets and extracts several facts used by the other rules.
    /// </summary>
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
    /// Based on analysis of ARP communication, the LAN prefix is inferred.
    /// </summary>
    /// <remarks>
    /// LAN prefic is computed as the longes prefix which includes all addresses considered 
    /// to be local addresses.
    /// </remarks>
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
    /// Classify the address as remote, local or multicast.
    /// </summary>
    public class GroupAddressesByScopeRule : DistanceRule
    {
        public override void Define()
        {
            LocalNetworkPrefix localNetworkPrefix = null;
            IGrouping<AddressScope, AddressMapping> addressClass = null;

            When()
                .Match(() => localNetworkPrefix)
                .Query(() => addressClass, q => q
                    .Match<AddressMapping>()
                    .GroupBy(m =>  LanRulesHelper.GetAddressScope(m.IpAddr, localNetworkPrefix)));
            Then()
                .Do(ctx => ctx.Info($"{addressClass.Key} hosts: {StringUtils.ToString(addressClass.ToArray())}"));
        }
    }

    /// <summary>
    /// Detects duplicate ip address by checking all address mappings.
    /// Single IP address should be mapped to a single hardware address only.
    /// </summary>
    public class DuplicateAddressRule : DistanceRule
    {
        public override void Define()
        {
            IGrouping<string, AddressMapping> group = null;
            When()
                .Query(() => group, q =>
                    q.Match<AddressMapping>()
                    .GroupBy(m => m.IpAddr)
                    .Where(g => g.Count() > 1));
            Then()
                 .Yield(_ => new IpAddressConflict { IpAddress = group.Key, EthAddresses = group.Select(x => x.EthAddr).ToArray() });
        }
    }

    //  
    /// <summary>
    /// A local host has an address that is not from LAN address scope. 
    /// </summary>
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
        
        public override void Define()
        {
            IpSourceEndpoint ipSrc = null;
            AddressMapping mapping = null;
            When()
                .Match(() => ipSrc, x => LanRulesHelper.IsLinkLocal(x.IpAddr))
                .Match(() => mapping, m => m.IpAddr == ipSrc.IpAddr);
            Then()
                 .Yield(_ => new LinkLocalIpAddressUse { IpAddress = ipSrc.IpAddr, EthAddress = mapping.EthAddr });
        }
    }

    /// <summary>
    /// Can be detected by checking LAN broadcasts. It is ok, if there is only one LAN broadcast.
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

    /// <summary>
    /// Collects all ip addresses of hosts that send to the same broadcast address.
    /// </summary>
    public class CollectBroadcastersRule : DistanceRule
    { 
        public override void Define()
        {
            LocalNetworkBroadcast bcast = null; 
            IEnumerable<IpFlow> flows = null;
            When()
                .Match(() => bcast)
                .Query(() => flows, q =>
                    q.Match<IpFlow>()
                    .Where(f => f.IpDst == bcast.IpBroadcast) 
                    .Collect());
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

    /// <summary>
    /// Invalid gateway is detected if some node has only local network communication and 
    /// ARP to translate address of potential gateway failed. 
    /// </summary>
    public class InvalidGatewayAddressRule : DistanceRule
    {
        
        public override void Define()
        {
            LocalNetworkPrefix localPrefix = null;
            ArpUnanswered hostArp = null;
            When()
                .Match(() => localPrefix)
                .Match(() => hostArp)
                .All<IpFlow>(f => f.IpSrc != hostArp.Request.ArpSrcProtoIpv4 || IsLocalOrMulticast(f.IpDst, localPrefix));
            Then()
                .Do(ctx => EmitInvalidGateway(ctx, hostArp));
        }
        bool IsLocalOrMulticast(string ipAddr, LocalNetworkPrefix localNetworkPrefix)
        {
            var scope = LanRulesHelper.GetAddressScope(ipAddr, localNetworkPrefix);
            return scope != AddressScope.RemoteAddress;
        }
        private void EmitInvalidGateway(IContext ctx, ArpUnanswered badGwArp)
        {
            ctx.TryInsert(new InvalidGateway { HostIpAddr = badGwArp.Request.ArpSrcProtoIpv4, GwIpAddr = badGwArp.Request.ArpDstProtoIpv4 });    
        }
    }

}