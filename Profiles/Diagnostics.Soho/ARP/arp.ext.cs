
namespace Distance.Diagnostics.Arp
{
    using Distance.Runtime;
    using Distance.Utils;
    using System;

    public enum ArpOpcode { Request = 1, Reply = 2 };
    public partial class ArpPacket
    {
        public bool IsGratuitous => this.ArpSrcProtoIpv4 == this.ArpDstProtoIpv4;
        public ArpOpcode Opcode => (ArpOpcode)this.ArpOpcode;
    }
}