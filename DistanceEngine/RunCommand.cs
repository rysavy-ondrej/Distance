using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Distance.Domain;
using Distance.Domain.Dns;
using Distance.Rules.Dns;
using Distance.Shark;
using Microsoft.Extensions.CommandLineUtils;
using NRules;
using NRules.Fluent;
using SharpPcap;
using SharpPcap.LibPcap;

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


        public static IEnumerable<string> ExecTShark(string inputfile, string protocol, params string[] fields)
        {
            var fieldString = String.Join(" -e ", fields);
            var arguments = $"-r {inputfile} -Y {protocol} -T fields -e {fieldString}";

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

        public void GenerateProtocolData()
        {

        }


        public int AnalyzeInput(string input)
        {
            if (!File.Exists(input)) throw new ArgumentException($"File '{input}' does not exist.");
            var pcapPath = Path.GetFullPath(input);
            var logPath = Path.ChangeExtension(pcapPath, "log");

            Program.ConfigureLog(logPath);
            var sw = new Stopwatch();
            sw.Start();
            
            Console.Write($"Loading and decoding packets from '{pcapPath}'...");
            var fields = DnsModel.Fields;
            var protocol = DnsModel.Protocol;
            var dnsPackets = ExecTShark(pcapPath, protocol, fields).Select(DnsModel.CreateFromLine).ToList();
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
            Console.Write($"Inserting facts ({dnsPackets.Count}) to the session...");
            session.InsertAll(dnsPackets);
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

        private PacketModel packetCreator(string protocols, IDictionary<string, object> fields)
        {
            if (protocols.Contains("dns"))
            {
                return new DnsModel(fields);
            }
            return new GenericPacketModel(fields);
        }
    }
}