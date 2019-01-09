using Distance.Runtime;
using Microsoft.Extensions.CommandLineUtils;
using NLog;
using NRules;
using NRules.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Distance.Engine
{
    internal class RunCommand
    {
        private Program.Options options;

        private Assembly m_diagnosticProfileAssembly;
        private int m_degreeOfParallelism = -1;
        public RunCommand(Program.Options options)
        {
            this.options = options;
        }

        public static string Name => "run";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Run a distance ruleset against the specified input file(s).";
            command.HelpOption("-?|-help");
            var profileAssemblyOption = command.Option("-profile <PROFILENAME>", "Specifies assembly that contains a diagnostic profile.", CommandOptionType.SingleValue);
            var parallelOption = command.Option("-parallel <NUMBER>", "Sets the degree of parallelism when loading and decoding of input data (-1 means unlimited).", CommandOptionType.SingleValue);
            var inputFile = command.Argument("InputPcapFile",
                "An input packet capture file to analyze.", false);

            command.OnExecute(() =>
            {
                if (parallelOption.HasValue()) m_degreeOfParallelism = Int32.Parse(parallelOption.Value());
                if (!profileAssemblyOption.HasValue())
                {
                        throw new Microsoft.Extensions.CommandLineUtils.CommandParsingException(command, "Required options '-profile' is missing.");
                }
                m_diagnosticProfileAssembly = Assembly.LoadFrom(GetAssemblyPath(profileAssemblyOption.Value()));
                return AnalyzeInput(inputFile.Value);
            });            
        }

        public static string GetAssemblyPath(string profileName)
        {
            if (!profileName.EndsWith(".dll"))
            {
                profileName += ".dll";
            }

            var fullpath = Path.GetFullPath(profileName);
            if (File.Exists(fullpath))
            {
                return Path.GetFullPath(profileName);
            }
            var profilePathsString = Environment.GetEnvironmentVariable("DISTANCE_PROFILES");
            if (profilePathsString != null)
            {
                var profilePaths = profilePathsString.Split(Path.PathSeparator);
                foreach(var path in profilePaths)
                {
                    fullpath = Path.Combine(path, profileName);
                    if (File.Exists(fullpath)) return fullpath;                        
                }
            }
            
            throw new FileNotFoundException($"Could not locate assembly file '{profileName}'.", profileName);
        }

        public static IEnumerable<string> RunShark(string inputfile, string filter, params string[] fields)
        {
            var fieldString = String.Join(" -e ", fields);
            var arguments = $"-r {inputfile} -Y {filter} -T fields -e {fieldString}";

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "tshark",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            while (true)
            {
                string line = process.StandardOutput.ReadLine();
                if (line != null)
                {
                    yield return line;
                }
                else
                {
                    break;
                }
            }

            process.WaitForExit();
        }

        public static readonly Char Separator = '\t';

        public IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t));
        }

        public IEnumerable<T> LoadFacts<T>(string pcapPath, string filter, string[]fields, Func<string[], T> creator)
        {
            return RunShark(pcapPath, filter, fields).Select(arg => creator(arg.Split(Separator)));
        }

        public IEnumerable<object> LoadFacts(string pcapPath, string filter, string[] fields, Func<string[], object> creator)
        {
            return RunShark(pcapPath, filter, fields).Select(arg => creator(arg.Split(Separator)));
        }

        public int AnalyzeInput(string input)
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

            var repository = new RuleRepository();
            Console.Write($"Loading rules from assembly '{m_diagnosticProfileAssembly.FullName}'...");
            repository.Load(x => x.From(Assembly.GetExecutingAssembly(), m_diagnosticProfileAssembly));
            Console.WriteLine($"ok [{sw.Elapsed}].");
            foreach(var rule in repository.GetRules())
            {
                logger.Info($"Rule: name={rule.Name}, priority = {rule.Priority}");
            }

            sw.Restart();
            Console.Write("Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.Write("Creating a session...");
            var session = factory.CreateSession();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();

            var facts = FindDerivedTypes(m_diagnosticProfileAssembly, typeof(DistanceFact));

            Console.WriteLine($"Loading facts, using {(m_degreeOfParallelism == -1 ? "all" : m_degreeOfParallelism.ToString() )} thread(s):");
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
                if (fired == 0) break;
                else Console.Write(".");
            }
            Console.WriteLine($"done [{sw.Elapsed}].");
            Console.WriteLine($"Diagnostic Log written to '{logPath}'.");
            Console.WriteLine($"Diagnostic Events written to '{eventPath}'.");
            return 0;
        }

        private void ExtractTransformLoad(Type factType, string pcapPath, ISession session)
        {
            var sw = new Stopwatch();
            sw.Start();
            var filter = (string)factType.GetField("Filter").GetValue(null);
            var fields = (string[])factType.GetField("Fields").GetValue(null);
            var createMethod = factType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            Console.WriteLine($"  Loading packets from '{pcapPath}', filter='{filter}'...");
            var factObjects = LoadFacts(pcapPath, filter, fields, f => createMethod.Invoke(null, new[] { f })).ToList();
            Console.WriteLine($"  ok [{sw.Elapsed}].");
            sw.Restart();
            Console.WriteLine($"  Inserting '{factType.Name}' facts ({factObjects.Count}) to the session.");
            session.InsertAll(factObjects);
            Console.WriteLine($"  ok [{sw.Elapsed}].");
        }
    }
}