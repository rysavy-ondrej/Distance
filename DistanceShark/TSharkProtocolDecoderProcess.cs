using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Distance.Shark
{
    public class TSharkProtocolDecoderProcess<Packet> : TSharkProcess<Packet>
    {
        private readonly Func<string, IDictionary<string, object>, Packet> m_packetCreator;
        private string[] m_protocols;
        /// <summary>
        /// Gets a collection of fields that is to be exported by TSHARK.
        /// </summary>
        public string[] Protocols => m_protocols;

        public TSharkProtocolDecoderProcess(Func<string, IDictionary<string, object>, Packet> packetCreator, params string[] protocols) : base()
        {
            m_packetCreator = packetCreator;
            m_protocols = protocols;
        }

        protected override string GetOutputFilter()
        {
            var protocols = String.Join(" ", m_protocols);
            return $"-J \"frame {protocols}\"";
        }

        protected override Packet GetResult(string line)
        {
            return DecodeJsonLine(m_protocols, line);
        }

        public Packet DecodeJsonLine(IEnumerable<string> protocols, string line)
        { 
            var jsonLine = JToken.Parse(line);
            var jsonLayer = jsonLine["layers"]; 
            var jsonFrame = jsonLayer["frame"];

            var packetFields = new Dictionary<string,object> ();

            packetFields["timestamp"] = (long)jsonLine["timestamp"];
            packetFields["frame_frame_number"] = (int)jsonFrame["frame_frame_number"];
            var frameProtocols = (string)jsonFrame["frame_frame_protocols"];
            packetFields["frame_frame_protocols"] = frameProtocols;

            foreach (var protocol in protocols)
            {
                var jsonProtocol = jsonLayer[protocol];                
                if (jsonProtocol!=null)
                {
                    foreach (var jsonField in jsonProtocol)
                    {
                        switch (jsonField)
                        {
                            case JProperty property:
                                if (property.Value.Type == JTokenType.String)
                                {
                                    packetFields[property.Name] = (string)property.Value;
                                }
                                break;
                            case JArray array:
                                // ????
                                break;
                        }                    
                    }
                }                       
            }
            return m_packetCreator(frameProtocols, packetFields);
        }
    }
}
