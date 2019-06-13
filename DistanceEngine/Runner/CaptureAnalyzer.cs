using Distance.Runtime;
using NLog;
using NRules;
using NRules.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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

        public async Task AnalyzeCaptureFile(string input)
        {
            if (!File.Exists(input)) throw new ArgumentException($"File '{input}' does not exist.");
            var pcapPath = Path.GetFullPath(input);
            var logPath = Path.ChangeExtension(pcapPath, "log");
            if (File.Exists(logPath)) File.Delete(logPath);
            var eventPath = Path.ChangeExtension(pcapPath, "evt");
            if (File.Exists(eventPath)) File.Delete(eventPath);

            Context.ConfigureLog(logPath, eventPath);
            var logger = LogManager.GetLogger(Context.DistanceOutputLoggerName);

            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("┌ Initializing repo...");
            var sessionFactory = CreateRepository(logger);
            Console.WriteLine($"└ ok [{sw.Elapsed}].");
            sw.Restart();
            Console.WriteLine("┌ Creating a session...");
            var session = sessionFactory.CreateSession();
            Console.WriteLine($"└ ok [{sw.Elapsed}].");
            sw.Restart();
            Console.WriteLine("┌ Processing...");

            await LoadFactsAsync(pcapPath, session);

            await FireRules(session);

            Console.WriteLine($"├─ Diagnostic Log written to '{logPath}'.");
            Console.WriteLine($"├─ Diagnostic Events written to '{eventPath}'.");
            Console.WriteLine($"└ done [{sw.Elapsed}].");
        }

        private async Task FireRules(ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("│┌ Firing rules:");
            var totalRulesFired = 0;
            var firedAccumulated = 0;
            var tmr = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
            var cts = new CancellationTokenSource();
            tmr.Subscribe(x => { Console.WriteLine($"│├─ fired: {firedAccumulated} rules [{sw.Elapsed}]."); firedAccumulated = 0; }, cts.Token);
            while (true)
            {
                var fired = session.Fire(1000);
                totalRulesFired += fired;
                if (fired == 0)
                {
                    cts.Cancel();
                    break;
                }
                else
                {
                    firedAccumulated += fired;
                }
            }
            Console.WriteLine($"│├─ fired: {firedAccumulated} rules.");
            Console.WriteLine($"│├ total rules fired: {totalRulesFired} at average rate: {(int)(totalRulesFired / sw.Elapsed.TotalSeconds)} rules/second.");
            Console.WriteLine($"│└ done [{sw.Elapsed}].");
        }

        private async Task<int> LoadFactsAsync(string pcapPath, ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"│┌ Loading facts from '{pcapPath}':");
            var facts = m_diagnosticProfileAssemblies.SelectMany(x => FactsLoaderFactory.FindDerivedTypes(x, typeof(DistanceFact))).ToList();
            var factsLoader = FactsLoaderFactory.Create<SharkFactsLoader>(facts, 
                info => Console.WriteLine($"│├─ start loading {info.FactType.Name} facts."), 
                (info, count) => Console.WriteLine($"│├─ stop loading {info.FactType.Name} facts ({count})."));

            int allFactsCount = 0;
            await factsLoader.GetData(pcapPath).ForEachAsync(obj => { allFactsCount += session.TryInsert(obj) ? 1 : 0; });
            Console.WriteLine($"│├ {allFactsCount} facts loaded.");
            Console.WriteLine($"│└ ok [{sw.Elapsed}].");
            return allFactsCount;
        }

        private ISessionFactory CreateRepository(Logger logger)
        {
            var sw = new Stopwatch();
            sw.Start();
            var repository = new RuleRepository();
            foreach (var assembly in m_diagnosticProfileAssemblies)
            {
                Console.WriteLine($"│┌ Loading rules from assembly '{assembly.Location}':");
                repository.Load(x => x.From(Assembly.GetExecutingAssembly(), assembly));
                foreach (var rule in repository.GetRules())
                {
                    Console.WriteLine($"│├─ {rule.Name} (pri={rule.Priority})");
                }
                Console.WriteLine($"│└ ok [{sw.Elapsed}].");
            }
            sw.Restart();
            Console.WriteLine("│┌ Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine($"│└ ok [{sw.Elapsed}].");
            return factory;
        }
    }
}
