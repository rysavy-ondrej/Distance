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

namespace Distance.Engine
{
    internal class RunCommand
    {
        private Program.Options options;

        private Assembly diagnosticProfileAssembly;
        public RunCommand(Program.Options options)
        {
            this.options = options;
        }

        public static string Name => "run";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Run a distance ruleset against the specified input file(s).";
            command.HelpOption("-?|-help");
            var profileAssembly = command.Option("-profile", "Specifies assembly that contains a diagnostic profile.", CommandOptionType.SingleValue);
            var inputFile = command.Argument("InputPcapFile",
                "An input packet capture file to analyze.", false);

            command.OnExecute(() =>
            {
                if (!profileAssembly.HasValue())
                {
                        throw new Microsoft.Extensions.CommandLineUtils.CommandParsingException(command, "Required options '-profile' is missing.");
                }
                diagnosticProfileAssembly = Assembly.LoadFrom(profileAssembly.Value());
                return AnalyzeInput(inputFile.Value);
            });            
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
            Console.Write($"Loading rules from assembly '{diagnosticProfileAssembly.FullName}'...");
            repository.Load(x => x.From(Assembly.GetExecutingAssembly(), diagnosticProfileAssembly));
            Console.WriteLine($"ok [{sw.Elapsed}].");
            foreach(var rule in repository.GetRules())
            {
                logger.Info($"Rule: name={rule.Name}, priority = {rule.Priority}");
            }

            // TODO: Turn the following block to Fact Loader implementation:
            // Facts definitions are stored in yaml file definition.
            //
            // The compiler generates Domain files for facts. 
            //
            // To load facts we search assemblies for all facts then apply the 
            // filter and generate loaders.
            //
            //
            //
            //
            sw.Restart();
            Console.Write("Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.Write("Creating a session...");
            var session = factory.CreateSession();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();

            
            
            var facts = FindDerivedTypes(diagnosticProfileAssembly, typeof(DistanceFact));

            foreach(var factType in facts)
            {
                var filter = (String)factType.GetField("Filter").GetValue(null);
                var fields = (string[])factType.GetField("Fields").GetValue(null);
                var createMethod = factType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                Console.Write($"Loading packets from '{pcapPath}', filter='{filter}'...");
                var factObjects = LoadFacts(pcapPath, filter, fields, f => createMethod.Invoke(null, new[] { f })).ToList();
                Console.WriteLine($"ok [{sw.Elapsed}].");
                sw.Restart();
                Console.Write($"Inserting '{factType.Name}' facts ({factObjects.Count}) to the session...");
                session.InsertAll(factObjects);
                Console.WriteLine($"ok [{sw.Elapsed}].");
            }

            sw.Restart();
            Console.Write("Waiting for completion...");
            //Start match/resolve/act cycle
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

    }
}