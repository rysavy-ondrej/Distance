using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Distance.Runtime
{
    public abstract class DistanceFact
    {
    }

    public class DistanceFactFactory<T> : DistanceFactFactoryBase
    {
        public DistanceFactFactory() : base(typeof(T))
        {
        }
    }

    public class DistanceFactFactoryBase
    {
        protected readonly MethodInfo m_createMethod;
        public DistanceFactFactoryBase(Type type)
        {
            m_createMethod = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        }
        public virtual object Create(string[] values)
        {
            return m_createMethod?.Invoke(null, new object[] { (Func<string,string,string>)Mapper, values });
        }
        protected virtual string Mapper(string fieldName, string value)
        {
            return value;
        }
    }
}
