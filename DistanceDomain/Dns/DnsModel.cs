using Newtonsoft.Json.Linq;
using System;
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
    /// tshark -r {} -T fields -e frame.number -e ip.src -e ip.dst -e dns.flags.response -e dns.id -e dns.flags.rcode -e dns.time -e dns.qry.name
    /// </remarks>
    public class DnsModel : PacketModel
    {
        public static readonly string[] Fields = new[] {
            "frame.number",
            "ip.src",
            "ip.dst",
            "dns.flags.response",
            "dns.id",
            "dns.flags.rcode",
            "dns.time",
            "dns.qry.name"
        };
        public static readonly string Protocol = "dns";

        public DnsModel(IDictionary<string, object> fields) : base(fields)
        {
        }

        public DnsModel(IEnumerable<KeyValuePair<string, string>> fields) : base(fields)
        {
        }

        public string DnsFlagsResponse => this["dns.flags.response"]?.ToString() ?? "0";
        public string DnsId => this["dns.id"]?.ToString() ?? "";
        public string DnsFlagsRcode => this["dns.flags.rcode"]?.ToString() ?? "0";
        public string DnsTime => this["dns.time"]?.ToString() ?? "0";
        public string DnsQryName => this["dns.qry.name"]?.ToString() ?? "";

        public override string ToString()
        {
            return $"[Dns: frame.number={FrameNumber} ip.src={IpSrc} ip.dst={IpDst} dns.flags.response={DnsFlagsResponse} dns.id={DnsId} dns.qry.name='{DnsQryName}' dns.time={DnsTime} dns.flags.rcode={DnsFlagsRcode}]";
        }

        public static DnsModel CreateFromLine(string arg)
        {
            var parts = arg.Split(Separator);
            var fields = Fields.Zip(parts, (k, v) => new KeyValuePair<string,string>(k, v));
            return new DnsModel(fields);
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
