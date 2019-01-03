using System;
using Distance.Runtime;
using Distance.Utils;
namespace Distance.Diagnostics.Dns
{
    public class DnsPacket
    {
        public static string Filter = "dns";
        public static string[] Fields = { "frame.number","ip.src","ip.dst","dns.id","dns.flags.response","dns.flags.rcode","dns.time","dns.qry.name" };
        [FieldName("frame.number")]
        public int FrameNumber { get; set; }
        [FieldName("ip.src")]
        public string IpSrc { get; set; }
        [FieldName("ip.dst")]
        public string IpDst { get; set; }
        [FieldName("dns.id")]
        public string DnsId { get; set; }
        [FieldName("dns.flags.response")]
        public bool DnsFlagsResponse { get; set; }
        [FieldName("dns.flags.rcode")]
        public int DnsFlagsRcode { get; set; }
        [FieldName("dns.time")]
        public double DnsTime { get; set; }
        [FieldName("dns.qry.name")]
        public string DnsQryName { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is DnsPacket that) && Equals(this.FrameNumber, that.FrameNumber) && Equals(this.IpSrc, that.IpSrc) && Equals(this.IpDst, that.IpDst) && Equals(this.DnsId, that.DnsId) && Equals(this.DnsFlagsResponse, that.DnsFlagsResponse) && Equals(this.DnsFlagsRcode, that.DnsFlagsRcode) && Equals(this.DnsTime, that.DnsTime) && Equals(this.DnsQryName, that.DnsQryName);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(FrameNumber,IpSrc,IpDst,DnsId,DnsFlagsResponse,DnsFlagsRcode,DnsTime,DnsQryName);
        public override string ToString()
        {
            return $"DnsPacket: frame.number={FrameNumber} ip.src={IpSrc} ip.dst={IpDst} dns.id={DnsId} dns.flags.response={DnsFlagsResponse} dns.flags.rcode={DnsFlagsRcode} dns.time={DnsTime} dns.qry.name={DnsQryName}";
        }
        public static DnsPacket Create(string[] values)
        {
            return new DnsPacket
            {
                FrameNumber = values[0].ToInt(),
                IpSrc = values[1].ToString(),
                IpDst = values[2].ToString(),
                DnsId = values[3].ToString(),
                DnsFlagsResponse = values[4].ToBool(),
                DnsFlagsRcode = values[5].ToInt(),
                DnsTime = values[6].ToDouble(),
                DnsQryName = values[7].ToString(),
            };
        }
    }
    public class QueryResponse
    {
        [FieldName("query")]
        public DnsPacket Query { get; set; }
        [FieldName("response")]
        public DnsPacket Response { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is QueryResponse that) && Equals(this.Query, that.Query) && Equals(this.Response, that.Response);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Query,Response);
        public override string ToString()
        {
            return $"QueryResponse: query={Query} response={Response}";
        }
    }
    public class ResponseError
    {
        [FieldName("query")]
        public DnsPacket Query { get; set; }
        [FieldName("response")]
        public DnsPacket Response { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is ResponseError that) && Equals(this.Query, that.Query) && Equals(this.Response, that.Response);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Query,Response);
        public override string ToString()
        {
            return $"ResponseError: query={Query} response={Response}";
        }
    }
    public class NoResponse
    {
        [FieldName("query")]
        public DnsPacket Query { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is NoResponse that) && Equals(this.Query, that.Query);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Query);
        public override string ToString()
        {
            return $"NoResponse: query={Query}";
        }
    }
    public class LateResponse
    {
        [FieldName("query")]
        public DnsPacket Query { get; set; }
        [FieldName("response")]
        public DnsPacket Response { get; set; }
        [FieldName("delay")]
        public double Delay { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is LateResponse that) && Equals(this.Query, that.Query) && Equals(this.Response, that.Response) && Equals(this.Delay, that.Delay);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(Query,Response,Delay);
        public override string ToString()
        {
            return $"LateResponse: query={Query} response={Response} delay={Delay}";
        }
    }
    public class DnsServer
    {
        [FieldName("ip.address")]
        public string IpAddress { get; set; }
        public override bool Equals(object obj)
        {
            return (obj is DnsServer that) && Equals(this.IpAddress, that.IpAddress);
        }
        public override int GetHashCode() => HashFunction.GetHashCode(IpAddress);
        public override string ToString()
        {
            return $"DnsServer: ip.address={IpAddress}";
        }
    }
}
