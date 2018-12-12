using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Distance.Domain.Dns
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Generate the source data by the following command:
    /// tshark -r only_dns.pcap -T json -e frame.number -e ip.src -e ip.dst -e dns.flags.response -e dns.id -e dns.flags.rcode -e dns.time -e dns.qry.name
    /// </remarks>
    public class DnsModel : PacketModel
    {

        public DnsModel(IDictionary<string, object> fields) : base(fields)
        {
        }

        public string DnsFlagsResponse => this["dns_flags_dns_flags_response"]?.ToString() ?? "0";
        public string DnsId => this["dns_dns_id"]?.ToString() ?? "";
        public string DnsFlagsRcode => this["dns_flags_dns_flags_rcode"]?.ToString() ?? "0";
        public string DnsTime => this["dns_dns_time"]?.ToString() ?? "0";
        public string DnsQryName => this["text_text"]?.ToString() ?? "";

        public override string ToString()
        {
            return $"[Dns: frame.number={FrameNumber} ip.src={IpSrc} ip.dst={IpDst} dns.flags.response={DnsFlagsResponse} dns.id={DnsId} dns.qry.name='{DnsQryName}' dns.time={DnsTime} dns.flags.rcode={DnsFlagsRcode}]";
        }
    }

    public class DnsQueryResponseModel
    {
        public DnsModel Query { get; set; }
        public DnsModel Response { get; set; }
    }

    public class DnsResponseErrorModel
    {
        public DnsModel Query { get; set; }
        public DnsModel Response { get; set; }
    }

    public class DnsNoResponseModel
    {
        public DnsModel Query { get; set; }
    }
}
