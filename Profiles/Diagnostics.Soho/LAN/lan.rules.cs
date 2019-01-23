namespace Distance.Diagnostics.Lan
{
    using Distance.Runtime;
    using NRules.Fluent.Dsl;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AddressMappingRule : DistanceRule
    {
        public override void Define()
        {
            IpPacket packet = null;

            When()
                .Match(() => packet); // LocalNetworkAddress.Contains(packet.IpSrc)
            Then()
                .Do(ctx => ctx.TryInsert(new AddressMapping { IpAddr = packet.IpSrc, EthAddr = packet.EthSrc }));
        }
    }

    public class DuplicateAddressRule : DistanceRule
    {
        public override void Define()
        {
            AddressMapping mapping = null;
            IEnumerable<AddressMapping> conflicts = null;

            When()
               .Match(() => mapping)
               .Query(() => conflicts,
                    c => c.Match<AddressMapping>(
                         m => mapping.IpAddr == m.IpAddr,
                         m => mapping.EthAddr != m.EthAddr)
                         .Collect()
                         .Where(x => x.Any()));
                         
            Then()
                .Yield(_ => new DuplicateAddressDetected { IpAddress = mapping.IpAddr, EthAddresses = conflicts.Select(c=>c.EthAddr).ToArray() });
        }
    }
}