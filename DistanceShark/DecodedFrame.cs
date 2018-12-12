using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Distance.Shark
{

    public class Protocol : DynamicObject
    {
        private IDictionary<string, object> m_entries;

        public Protocol(IDictionary<string, object> entries)
        {
            this.m_entries = entries;
        }
        public Protocol()
        {
            this.m_entries = new Dictionary<string, object>();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name.ToLowerInvariant();

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return m_entries.TryGetValue(name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            m_entries[binder.Name.ToLowerInvariant()] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }

    /// <summary>
    /// This class represents decoded frames. Tshark can be used to parse frames to JSON output, which is used to create objects of this class. 
    /// </summary>
    public class Packet
    {
        dynamic m_fields;
        public Packet(dynamic fields)
        {
            m_fields = fields;
        }
        public override string ToString()
        {
            return $"[Packet frame.number={FrameNumber} frame.time={Timestamp} frame.protocols={FrameProtocols}]";
        }

        public long Timestamp => m_fields.timestamp;
        public int FrameNumber => m_fields.frame_frame_number;
        public string FrameProtocols => m_fields.frame_frame_protocols;
    }
}
