using System;

namespace Distance.Runtime
{
    public class FieldNameAttribute : Attribute
    {
        private readonly string m_value;

        public FieldNameAttribute(string value)
        {
            m_value = value;
        }

        public string Value => m_value;
    }
}