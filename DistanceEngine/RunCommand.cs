using Distance.Diagnostics.Dns;
using Distance.Diagnostics.Icmp;
using Distance.Diagnostics.Lan;
using Distance.Runtime;
using Microsoft.Extensions.CommandLineUtils;
using NRules;
using NRules.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Distance.Engine
{
    internal class RunCommand
    {
        private Program.Options options;

        public RunCommand(Program.Options options)
        {
            this.options = options;
        }

        public static string Name => "run";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Run a distance ruleset against the specified input file(s).";
            command.HelpOption("-?|-help");
            var inputFile = command.Argument("InputPcapFile",
                "An input packet capture file to analyze.", false);

            command.OnExecute(() =>
            {
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

        public IEnumerable<T> LoadFacts<T>(string pcapPath, string filter, string[]fields, Func<string[], T> creator)
        {
            return RunShark(pcapPath, filter, fields).Select(arg => creator(arg.Split(Separator)));
        }

        public int AnalyzeInput(string input)
        {
            if (!File.Exists(input)) throw new ArgumentException($"File '{input}' does not exist.");
            var pcapPath = Path.GetFullPath(input);
            var logPath = Path.ChangeExtension(pcapPath, "log");
            var eventPath = Path.ChangeExtension(pcapPath, "log"); 
            if (File.Exists(logPath)) File.Delete(logPath);

            Context.ConfigureLog(logPath);
            var sw = new Stopwatch();
            sw.Start();

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
            Console.Write($"Loading and decoding packets from '{pcapPath}'...");
            var dnsPackets = LoadFacts<DnsPacket>(pcapPath, DnsPacket.Filter, DnsPacket.Fields, DnsPacket.Create).ToList();
            var icmpPackets = LoadFacts<IcmpPacket>(pcapPath, IcmpPacket.Filter, IcmpPacket.Fields, IcmpPacket.Create).ToList();
            var ipPackets = LoadFacts<IpPacket>(pcapPath, IpPacket.Filter, IpPacket.Fields, IpPacket.Create).ToList();
            Console.WriteLine($"ok [{sw.Elapsed}].");

            sw.Restart();
            var repository = new RuleRepository();
            var assembly = typeof(DnsRequestResponseRule).Assembly;
            Console.Write($"Loading rules from assembly '{assembly.FullName}'...");
            repository.Load(x => x.From(assembly));
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.Write("Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.Write("Creating a session...");
            var session = factory.CreateSession();
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.Write($"Inserting 'DnsPacket' facts ({dnsPackets.Count}) to the session...");
            session.InsertAll(dnsPackets);
            Console.WriteLine($"ok [{sw.Elapsed}].");

            sw.Restart();
            Console.Write($"Inserting 'IcmpPacket' facts ({icmpPackets.Count}) to the session...");
            session.InsertAll(icmpPackets);
            Console.WriteLine($"ok [{sw.Elapsed}].");

            sw.Restart();
            Console.Write($"Inserting 'IpPacket' facts ({ipPackets.Count}) to the session...");
            session.InsertAll(ipPackets);
            Console.WriteLine($"ok [{sw.Elapsed}].");

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
            Console.WriteLine($"Diagnostic output written to '{logPath}'.");
            return 0;
        }

    }
}