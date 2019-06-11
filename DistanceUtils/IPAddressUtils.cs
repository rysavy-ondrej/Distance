using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Distance.Utils
{
    public static class IPAddressUtils
    {
        public static (IPAddress, int) CommonPrefix(IPAddress address1, int prefix1, IPAddress address2, int prefix2)
        {
            var bytes1 = address1.GetAddressBytes().Reverse().ToArray();
            var bytes2 = address2.GetAddressBytes().Reverse().ToArray();
            var x1 = BitConverter.ToUInt32(bytes1, 0);
            var x2 = BitConverter.ToUInt32(bytes2, 0);
            int len = 0;
            while (len < Math.Min(prefix1, prefix2) && ((x1 & 0x80000000) == (x2 & 0x80000000)))
            {
                x1 = x1 << 1;
                x2 = x2 << 1;
                len++;
            }
            var x = BitConverter.ToUInt32(bytes1, 0) & (0xff_ff_ff_ff << 32 - len);
            var ip = new IPAddress(BitConverter.GetBytes(x).Reverse().ToArray());
            return (ip, len);
        }


        public static bool BelongsTo(this IPAddress address,  IPAddress network, int prefix)
        {
            if (prefix < 0 || prefix > 32) throw new ArgumentException("Value must be between 0 and 32.", nameof(prefix));
            var addressBytes = address.GetAddressBytes().Reverse().ToArray();
            var networkBytes = network.GetAddressBytes().Reverse().ToArray();
            var addressValue = BitConverter.ToUInt32(addressBytes, 0);
            var networkValue = BitConverter.ToUInt32(networkBytes, 0);
            var maskValue = (0xff_ff_ff_ff << 32 - prefix);
            return (addressValue & maskValue) == (networkValue & maskValue);
        }
    }
}
