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


        public int AnalyzeInput(string inputFilename)
        {
            if (!File.Exists(inputFilename)) throw new ArgumentException($"File '{inputFilename}' does not exist.");
            Program.ConfigureLog(Path.ChangeExtension(inputFilename, "log"));
            var sw = new Stopwatch();

            //--- LOADING PACKETS:
            var inputDevice = new CaptureFileReaderDevice(inputFilename);

            inputDevice.Open();
            var tsharkProcess = new TSharkProtocolDecoderProcess<PacketModel>(packetCreator, "icmp", "ip", "dns");

            IEnumerable<RawCapture> ReadAllFrames(CaptureFileReaderDevice input)
            {
                SharpPcap.RawCapture capture;
                while ((capture = input.GetNextPacket()) != null)
                {
                    yield return capture;
                }
            }
            sw.Restart();
            Console.Write($"Loading and decoding packets from '{inputFilename}'...");
            var frames = ReadAllFrames(inputDevice);
            var packets = TSharkDecoder.Decode(frames, tsharkProcess).ToList();
            inputDevice.Close();
            tsharkProcess.Close();
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
            Console.Write($"Inserting facts ({packets.Count}) to the session...");
            session.InsertAll(packets);
            Console.WriteLine($"ok [{sw.Elapsed}].");


            sw.Restart();
            Console.WriteLine("Starting the engine...");
            //Start match/resolve/act cycle
            var result = session.Fire();
            Console.WriteLine($"done [{sw.Elapsed}].");
            return result;
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