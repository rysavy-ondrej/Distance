using Distance.Runtime;
using NLog;
using NRules;
using NRules.Diagnostics;
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
using System.Xml;

namespace Distance.Engine.Runner
{
    public class CaptureAnalyzer
    {
        private Assembly[] _diagnosticProfileAssemblies;
        private int _degreeOfParallelism = -1;
        private ISession _session;

        public int DegreeOfParallelism
        {
            get => _degreeOfParallelism;
            set
            {
                if (value < -1 || value > 128) return;
                _degreeOfParallelism = value;
            }
        }

        /// <summary>
        /// Creates analyzer and loads diagnostic rules from the provided assemblies.
        /// </summary>
        /// <param name="diagnosticProfileAssemblies"></param>
        public CaptureAnalyzer(params Assembly[] diagnosticProfileAssemblies)
        {
            _diagnosticProfileAssemblies = diagnosticProfileAssemblies;
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("┌ Initializing repo...");
            var sessionFactory = CreateRepository();
            Console.WriteLine($"└ done [{sw.Elapsed}].");
            sw.Restart();
            Console.WriteLine("┌ Creating a _session...");
            _session = sessionFactory.CreateSession(OnInitialization);
            if (_session == null) throw new InvalidOperationException("Could not create analyzer.");
            Console.WriteLine($"└ done [{sw.Elapsed}].");            
        }

        private void OnInitialization(ISession obj)
        {            
            // do nothing at this moment...
        }

        public async Task AnalyzeCaptureFile(string input)
        {
            if (!File.Exists(input)) throw new ArgumentException($"File '{input}' does not exist.");
            var pcapPath = Path.GetFullPath(input);
            var logPath = Path.ChangeExtension(pcapPath, "log");
            if (File.Exists(logPath)) File.Delete(logPath);
            var eventPath = Path.ChangeExtension(pcapPath, "evt");
            if (File.Exists(eventPath)) File.Delete(eventPath);
            var dgmlPath = Path.ChangeExtension(pcapPath, "gexf");
            Context.ConfigureLog(logPath, eventPath);
            var logger = LogManager.GetLogger(Context.DistanceOutputLoggerName);

            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("┌ Processing...");

            await LoadFactsAsync(pcapPath, _session);

            await FireRulesAsync(_session);

            Console.WriteLine($"├─ Diagnostic Log written to '{logPath}'.");
            Console.WriteLine($"├─ Diagnostic Events written to '{eventPath}'.");
            Console.WriteLine($"└ done [{sw.Elapsed}].");
        }

        public void DumpReteToFile(string dgmlPath)
        {
            var session = (ISessionSnapshotProvider)_session;
            var snapshot = session.GetSnapshot();
            var dgmlWriter = new GraphWriter(snapshot);
            using (var xmlWriter = XmlWriter.Create(dgmlPath, new XmlWriterSettings {Indent = true, NewLineHandling = NewLineHandling.Entitize  }))
            {
                dgmlWriter.WriteTo(xmlWriter);
            }

        }

        private async Task FireRulesAsync(ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine("│┌ Firing rules:");
            var totalRulesFired = 0;
            var firedAccumulated = 0;
            var tmr = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000));
            var cts = new CancellationTokenSource();

            void PrintStatus(long x)
            {
                Console.WriteLine($"│├─ {sw.Elapsed}: fired {firedAccumulated}/{totalRulesFired} new/total rules.");
                firedAccumulated = 0;
            }

            tmr.Subscribe(PrintStatus, cts.Token);
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
            PrintStatus(0);
            Console.WriteLine($"│├ total rules fired: {totalRulesFired} at average rate: {(int)(totalRulesFired / sw.Elapsed.TotalSeconds)} rules/second.");
            Console.WriteLine($"│└ done [{sw.Elapsed}].");
            await Task.CompletedTask;
        }

        private async Task<int> LoadFactsAsync(string pcapPath, ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"│┌ Loading facts from '{pcapPath}':");
            var facts = _diagnosticProfileAssemblies.SelectMany(x => FactsLoaderFactory.FindDerivedTypes(x, typeof(DistanceFact))).ToList();
            var factsLoader = FactsLoaderFactory.Create<SharkFactsLoader>(facts,
                info => Console.WriteLine($"│├─ start loading {info.FactType.Name} facts."),
                (info, count) => Console.WriteLine($"│├─ stop loading {info.FactType.Name} facts ({count})."));

            int allFactsCount = 0;
            await factsLoader.GetData(pcapPath).ForEachAsync(obj => { allFactsCount += session.TryInsert(obj) ? 1 : 0; });
            Console.WriteLine($"│├ {allFactsCount} facts loaded.");
            Console.WriteLine($"│└ done [{sw.Elapsed}].");
            return allFactsCount;
        }

        private ISessionFactory CreateRepository()
        {
            var sw = new Stopwatch();
            sw.Start();
            var repository = new RuleRepository();
            foreach (var assembly in _diagnosticProfileAssemblies)
            {
                Console.WriteLine($"│┌ Loading rules from assembly '{assembly.Location}':");
                repository.Load(x => x.From(Assembly.GetExecutingAssembly(), assembly));
                foreach (var rule in repository.GetRules())
                {
                    Console.WriteLine($"│├─ {rule.Name} (pri={rule.Priority})");
                }
                Console.WriteLine($"│└ done [{sw.Elapsed}].");
            }
            sw.Restart();
            Console.WriteLine("│┌ Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine($"│└ done [{sw.Elapsed}].");
            return factory;
        }
    }
}
