using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Distance.Domain.Dns
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Generate the source data by the following command:
    /// tshark -r only_dns.pcap -T json -e frame.number -e ip.src -e ip.dst -e dns.flags.response -e dns.id -e dns.flags.rcode -e dns.time -e dns.qry.name
    /// </remarks>
    public class DnsModel
    {
        public string FrameNumber { get; set; }
        public string IpSrc { get; set; }
        public string IpDst { get; set; }
        public string DnsFlagsResponse { get; set; }
        public string DnsId { get; set; }
        public string DnsFlagsRcode { get; set; }
        public string DnsTime { get; set; }
        public string DnsQryName { get; set; }

        public override string ToString()
        {
            return $"[Dns: frame.number={FrameNumber} ip.src={IpSrc} ip.dst={IpDst} dns.flags.response={DnsFlagsResponse} dns.id={DnsId} dns.qry.name={DnsQryName} dns.time={DnsTime} dns.flags.rcode={DnsFlagsRcode}]";
        }

        /// <summary>
        /// Loads the collection of <see cref="DnsModel"/> from the tshark -T json file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<DnsModel> LoadFromJson(string path)
        {
            //            {
            //                "_index": "packets-2018-12-11",
            //                "_type": "pcap_file",
            //                "_score": null,
            //                "_source": {
            //                    "layers": {
            //                        "frame.number": ["23"],
            //                        "ip.src": ["192.168.5.122"],
            //                        "ip.dst": ["198.164.30.2"],
            //                        "dns.flags.response": ["0"],
            //                        "dns.id": ["0x0000a8d3"],
            //                        "dns.qry.name": ["yf6.yahoo.com"]
            //                     }
            //                 }
            //             }
            var json = File.ReadAllText(path);
            var jarray = JArray.Parse(json);
            return jarray.Select(x=> CreateFromJson(x["_source"]["layers"])).Where(x=>x!=null);
        }

        private static DnsModel CreateFromJson(JToken jToken)
        {
            if (jToken["dns.id"] == null) return null;
            var obj = new DnsModel {
                FrameNumber = jToken["frame.number"]?.First().ToString() ?? "",
                IpSrc = jToken["ip.src"]?.First().ToString() ?? "",
                IpDst = jToken["ip.dst"]?.First().ToString() ?? "",
                DnsFlagsResponse = jToken["dns.flags.response"]?.First().ToString() ?? "",
                DnsId = jToken["dns.id"]?.First().ToString() ?? "",
                DnsQryName = jToken["dns.qry.name"]?.First().ToString() ?? "",
                DnsFlagsRcode = jToken["dns.flags.rcode"]?.First().ToString() ?? "",
                DnsTime = jToken["dns.time"]?.First().ToString() ?? ""
            };
            return obj;
        }
    }


    public class DnsQueryResponse
    {
        public DnsModel Query { get; set; }
        public DnsModel Response { get; set; }
    }
}
