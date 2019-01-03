using System;
using Distance.Runtime;
using Distance.Utils;
namespace Distance.Diagnostics.Icmp
{
    public class IcmpPacket
    {
        public static string Filter = "icmp";
        public static string[] Fields = { "frame.number","ip.src","ip.dst","icmp.type","icmp.code","icmp.ident","icmp.seq" };
        [FieldName("frame.number")]
        public int FrameNumber { get; set; }
        [FieldName("ip.src")]
        public string IpSrc { get; set; }
        [FieldName("ip.dst")]
        public string IpDst { get; set; }
        [FieldName("icmp.type")]
        public int IcmpType { get; set; }
        [FieldName("icmp.code")]
        public int IcmpCode { get; set; }
        [FieldName("icmp.ident")]
        public int IcmpIdent { get; set; }
        [FieldName("icmp.seq")]
        public int IcmpSeq { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is IcmpPacket that) && Equals(this.FrameNumber, that.FrameNumber) && Equals(this.IpSrc, that.IpSrc) && Equals(this.IpDst, that.IpDst) && Equals(this.IcmpType, that.IcmpType) && Equals(this.IcmpCode, that.IcmpCode) && Equals(this.IcmpIdent, that.IcmpIdent) && Equals(this.IcmpSeq, that.IcmpSeq);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(FrameNumber,IpSrc,IpDst,IcmpType,IcmpCode,IcmpIdent,IcmpSeq);
        public override string ToString()
        {
            return $"IcmpPacket: frame.number={FrameNumber} ip.src={IpSrc} ip.dst={IpDst} icmp.type={IcmpType} icmp.code={IcmpCode} icmp.ident={IcmpIdent} icmp.seq={IcmpSeq}";
        }
        public static IcmpPacket Create(string[] values)
        {
            return new IcmpPacket
            {
                FrameNumber = values[0].ToInt(),
                IpSrc = values[1].ToString(),
                IpDst = values[2].ToString(),
                IcmpType = values[3].ToInt(),
                IcmpCode = values[4].ToInt(),
                IcmpIdent = values[5].ToInt(),
                IcmpSeq = values[6].ToInt(),
            };
        }
    }
    public class IcmpDestinationUnreachable
    {
        [FieldName("packet")]
        public IcmpPacket Packet { get; set; }
        [FieldName("code")]
        public int Code { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is IcmpDestinationUnreachable that) && Equals(this.Packet, that.Packet) && Equals(this.Code, that.Code);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Packet,Code);
        public override string ToString()
        {
            return $"IcmpDestinationUnreachable: packet={Packet} code={Code}";
        }
    }
    public class TtlExpired
    {
        [FieldName("packet")]
        public IcmpPacket Packet { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is TtlExpired that) && Equals(this.Packet, that.Packet);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Packet);
        public override string ToString()
        {
            return $"TtlExpired: packet={Packet}";
        }
    }
}
