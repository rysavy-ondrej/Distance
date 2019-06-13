using Distance.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Distance.Engine.Runner
{
    public class FactsLoaderFactory
    {
        public static IFactsLoader Create<T>(List<Type> facts, Action<FactsInformation> onStarted = null, Action<FactsInformation, int> onCompleted = null)
        {
            var factsInfoCollection = facts.Select(CreateInfoObject).Where(x => x != null).ToArray();
            if (typeof(T) == typeof(SharkFactsLoader)) return new SharkFactsLoader(factsInfoCollection, onStarted, onCompleted);
            return null;
        }

        public static FactsInformation CreateInfoObject(Type factType)
        {
            var filter = (string)factType.GetField("Filter")?.GetValue(null);
            var fields = (string[])factType.GetField("Fields")?.GetValue(null);
            var factory = GetFactoryObject(factType);
            if (filter != null && fields != null && factory != null)
            {
                return new FactsInformation(factType, filter, fields, factory.Create);
            }
            else
            {
                return null;
            }
        }

        public static Type GetFactoryType(Type factType)
        {
            var asm = Assembly.GetAssembly(factType);
            var genericType = typeof(DistanceFactFactory<object>).GetGenericTypeDefinition();
            var factoryType = genericType.MakeGenericType(factType);
            return FindDerivedTypes(asm, factoryType).FirstOrDefault() ?? factoryType;
        }

        public static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
        }

        public static DistanceFactFactoryBase GetFactoryObject(Type factType)
        {
            var factoryType = GetFactoryType(factType);
            var constructor = factoryType.GetConstructor(new Type[0]);
            var instance = constructor.Invoke(new object[0]);
            return (DistanceFactFactoryBase)instance;
        }
    }
}
