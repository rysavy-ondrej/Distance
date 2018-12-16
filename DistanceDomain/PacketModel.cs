using Distance.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Distance.Domain
{
    public abstract class PacketModel : DynamicObject
    {
        public static readonly Char Separator = '\t';
        private readonly IDictionary<string, object> m_fields;

        public int FrameNumber
        {
            get => this["frame.number"]?.To<int>() ?? 0;
            set { this["frame.number"] = value; }
        }
        public string IpSrc
        {
            get => this["ip.src"].ToString();
            set => this["ip.src"] = value;
        }
        public string IpDst
        {
            get => this["ip.dst"].ToString();
            set => this["ip.dst"] = value;
        }

        protected PacketModel(IDictionary<string,object> fields)
        {
            m_fields = fields;
        }

        protected PacketModel(IEnumerable<KeyValuePair<string,string>> fields)
        {
            m_fields = fields.ToDictionary(x=> x.Key, y => (object)y.Value);
        }

        protected PacketModel()
        {
            m_fields = new Dictionary<string, object>();
        }

        public object this[string name]
        {
            get
            {
                m_fields.TryGetValue(name, out object value);
                return value;
            }
            set
            {
                m_fields[name] = value;
            }
        }

        // This property returns the number of elements
        // in the inner dictionary.
        public int Count
        {
            get
            {
                return m_fields.Count;
            }
        }

        /// <summary>
        /// We compute the "canonical" name from the property name. 
        /// The property name uses Capitalization to sperate parts of the property path.
        /// For example, 'DnsId' is translated to 'dns.id'.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        string GetCannonicalName(string propertyName)
        {
            List<Char> cannonicalName = new List<char>();
            // replace all upper case with lower case preceding with '.'
            foreach(var ch in propertyName)
            {
                if (Char.IsUpper(ch))
                {
                    if (cannonicalName.Count != 0) cannonicalName.Add('.');
                    cannonicalName.Add(Char.ToLower(ch));
                }
                else
                    cannonicalName.Add(ch);
            }
            return new string(cannonicalName.ToArray());
        }


        // If you try to get a value of a property 
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive and wireshark compatible.
            string name = GetCannonicalName(binder.Name);

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return m_fields.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive and wireshark compatible.
            string name = GetCannonicalName(binder.Name);

            m_fields[name] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }

    public class GenericPacketModel : PacketModel
    {
        public GenericPacketModel(IDictionary<string, object> fields) : base(fields) { }
    }
}
