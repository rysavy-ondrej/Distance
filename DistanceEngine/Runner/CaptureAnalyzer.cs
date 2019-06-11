using Distance.Runtime;
using NLog;
using NRules;
using NRules.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distance.Engine.Runner
{
    public class CaptureAnalyzer
    {
        private Assembly[] m_diagnosticProfileAssemblies;
        private int m_degreeOfParallelism = -1;

        public int DegreeOfParallelism
        {
            get => m_degreeOfParallelism;
            set
            {
                if (value < -1 || value > 128) return;
                m_degreeOfParallelism = value;
            }
        }

        public CaptureAnalyzer(params Assembly[] diagnosticProfileAssemblies)
        {
            m_diagnosticProfileAssemblies = diagnosticProfileAssemblies;
        }

        public int AnalyzeCaptureFile(string input)
        {
            if (!File.Exists(input)) throw new ArgumentException($"File '{input}' does not exist.");
            var pcapPath = Path.GetFullPath(input);
            var logPath = Path.ChangeExtension(pcapPath, "log");
            var eventPath = Path.ChangeExtension(pcapPath, "evt");
            if (File.Exists(logPath)) File.Delete(logPath);

            Context.ConfigureLog(logPath, eventPath);
            var logger = LogManager.GetLogger(Context.DistanceOutputLoggerName);
            var sw = new Stopwatch();
            sw.Start();

            var factory = InitializeRepository(logger, sw);
            Console.WriteLine($"ok [{sw.Elapsed}].");
            sw.Restart();
            Console.Write("Creating a session...");
            var session = factory.CreateSession();
            Console.WriteLine($"ok [{sw.Elapsed}].");
            sw.Restart();

            var facts = m_diagnosticProfileAssemblies.SelectMany(x=> FindDerivedTypes(x, typeof(DistanceFact))).ToList();

            Console.WriteLine($"Loading facts, using {(m_degreeOfParallelism == -1 ? "all" : m_degreeOfParallelism.ToString())} thread(s):");
            Parallel.ForEach(facts, new ParallelOptions() { MaxDegreeOfParallelism = m_degreeOfParallelism }, (factType) =>
            {
                ExtractTransformLoad(factType, pcapPath, session);
            });

            Console.WriteLine($"All facts loaded [{sw.Elapsed}].");
            sw.Restart();
            Console.Write("Waiting for completion...");
            // start match/resolve/act cycle
            while (true)
            {
                var fired = session.Fire(100);
                if (fired == 0)
                {
                    break;
                }
                else
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine($"done [{sw.Elapsed}].");
            Console.WriteLine($"Diagnostic Log written to '{logPath}'.");
            Console.WriteLine($"Diagnostic Events written to '{eventPath}'.");
            return 0;
        }

        private ISessionFactory InitializeRepository(Logger logger, Stopwatch sw)
        {
            var repository = new RuleRepository();
            foreach (var assembly in m_diagnosticProfileAssemblies)
            {
                Console.Write($"Loading rules from assembly '{assembly.FullName}', Location='{assembly.Location}'...");
                repository.Load(x => x.From(Assembly.GetExecutingAssembly(), assembly));
                Console.WriteLine($"ok [{sw.Elapsed}].");

                foreach (var rule in repository.GetRules())
                {
                    logger.Info($"Rule: name={rule.Name}, priority = {rule.Priority}");
                }
            }
            sw.Restart();
            Console.Write("Compiling rules...");
            var factory = repository.Compile();
            return factory;
        }
        private Type GetFactoryType(Type factType)
        {
            var asm = Assembly.GetAssembly(factType);
            var genericType = typeof(DistanceFactFactory<object>).GetGenericTypeDefinition();
            var factoryType = genericType.MakeGenericType(factType);
            return FindDerivedTypes(asm, factoryType).FirstOrDefault() ?? factoryType;
        }

        private DistanceFactFactoryBase GetFactoryObject(Type factType)
        {
            var factoryType = GetFactoryType(factType);
            var constructor = factoryType.GetConstructor(new Type[0]);
            var instance = constructor.Invoke(new object[0]);
            return (DistanceFactFactoryBase)instance;
        }

        private void ExtractTransformLoad(Type factType, string pcapPath, ISession session)
        {

            var filter = (string)factType.GetField("Filter").GetValue(null);
            var fields = (string[])factType.GetField("Fields").GetValue(null);
            var factory = GetFactoryObject(factType);

            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"  Loading packets from '{pcapPath}', filter='{filter}'...");
            var factObjects = FactsLoader.Load(pcapPath, filter, fields, factory.Create).ToList();
            Console.WriteLine($"  ok [{sw.Elapsed}].");
            sw.Restart();
            Console.WriteLine($"  Inserting '{factType.Name}' facts ({factObjects.Count}) to the session.");
            session.InsertAll(factObjects);
            Console.WriteLine($"  ok [{sw.Elapsed}].");
            sw.Stop();
        }
        public IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
        }
    }
}
