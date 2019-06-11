using Distance.Diagnostics.Lan;
using System;
using System.Collections.Generic;
using System.Text;

namespace Distance.Diagnostics.Lan
{
    public class IpPacketFactory : Distance.Runtime.DistanceFactFactory<IpPacket>
    {
        protected override string Mapper(string fieldName, string value)
        {
            switch(fieldName)
            {
                case "ip.src":
                case "ip.dst":
                    return value.Split(',')[0];   
                default:
                    return base.Mapper(fieldName, value);
            }
            
        }
    }
}
