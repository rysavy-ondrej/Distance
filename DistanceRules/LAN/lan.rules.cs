namespace Distance.Diagnostics.Lan
{
    using Distance.Runtime;
    using Distance.Utils;
    using NRules.Fluent.Dsl;
    using System;

    public class DuplicateAddress : Rule
    {
        public override void Define()
        {
            IpPacket ipPacket1 = null;
            IpPacket ipPacket2 = null;

            When()
               .Match(() => ipPacket1, p => true)
               .Match(() => ipPacket2, p2 => ipPacket1.IpSrc == p2.IpSrc, p2 => ipPacket1.EthSrc != p2.EthSrc);

            Then()
                .Yield(_ => new DuplicateAddressDetected { Ip = ipPacket1.IpSrc, Eth1 = ipPacket1.EthSrc, Eth2 = ipPacket2.EthSrc });
        }
    }
}