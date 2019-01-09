namespace Distance.Diagnostics.Lan
{
    using Distance.Runtime;
    using Distance.Utils;
    using NRules.Fluent.Dsl;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DuplicateAddress : Rule
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