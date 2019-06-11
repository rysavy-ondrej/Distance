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
    using System.Linq.Expressions;
    using System.Net;

    public class CollectIpEndpointsRule : DistanceRule
    {
        public override void Define()
        {
            IpPacket packet = null;

            When()
                .Match(() => packet); 
            Then()
                .Do(ctx => ctx.TryInsert(new IpEndpoint { IpAddr = packet.IpSrc }))
                .Do(ctx => ctx.TryInsert(new IpEndpoint { IpAddr = packet.IpDst }));

        }
    }
    public class AddressMappingRule : DistanceRule
    {
        public override void Define()
        {
            IpPacket packet = null;

            When()
                .Match(() => packet); 
            Then()
                .Do(ctx => ctx.TryInsert(new AddressMapping { IpAddr = packet.IpSrc, EthAddr = packet.EthSrc }));
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
                 .Yield(_ => new IpAddressConflict { IpAddress = group.Key, EthAddresses = group.Select(x=>x.EthAddr).ToArray() });
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

            When()
                .Query(() => group, q =>
                    from m in q.Match<AddressMapping>()
                    group m by m.EthAddr into g
                    where g.Count() > 1
                    select g);
            Then()
                 .Yield(_ => new GatewayCandidate { EthAddr = group.Key });
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
            When()
                .Query(() => localAddresses, q => q
                    .Match<ArpAddressMapping>()
                    .Select(x => x.IpAddr)
                    .Collect());

            Then()
                .Do(ctx => ComputePrefixes(ctx, localAddresses));
        }

        private void ComputePrefixes(IContext ctx, IEnumerable<string> localAddresses)
        {
            if (localAddresses == null || localAddresses.Count() == 0) return;
            // compute wildcard:
            var wc = localAddresses.Select(s => (Address:IPAddress.Parse(s),Prefix:32)).Aggregate((x,y) => IPAddressUtils.CommonPrefix(x.Address, x.Prefix,y.Address, y.Prefix));
            Console.WriteLine($"Local prefix: {wc.Address}/{wc.Prefix}");
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
                .Do(ctx => Console.WriteLine($"{group.Key} hosts: {StringUtils.ToString(group.ToArray())}"));
        }
        string LocalOrRemote(LocalNetworkPrefix localNetworkPrefix, AddressMapping m)
        {
            return IPAddress.Parse(m.IpAddr).BelongsTo(IPAddress.Parse(localNetworkPrefix.IpNetwork), localNetworkPrefix.IpPrefix) ? "local" : "remote";
        }

    }
}