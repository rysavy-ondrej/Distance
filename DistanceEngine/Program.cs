using Distance.Domain;
using Distance.Domain.Dns;
using Distance.Rules;
using Distance.Rules.Dns;
using Distance.Shark;
using NLog;
using NRules;
using NRules.Fluent;
using NRules.RuleModel;
using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Distance.Engine
{
    class Program
    {
        static void ExecTime(string message, Action action)
        {
            var sw = new Stopwatch();
            Console.Write($"{message}...");
            sw.Start();
            action();
            sw.Stop();
            Console.WriteLine($"ok ({sw.Elapsed}).");
        }

        static void ConfigureLog(string filename, bool logToConsole = false)
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget() { FileName = filename };
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            if (logToConsole)
            {
                var logconsole = new NLog.Targets.ColoredConsoleTarget();
                config.AddRule(LogLevel.Warn, LogLevel.Fatal, logconsole);
            }
            
            LogManager.Configuration = config;
        }


        static void Main(string[] args)
        {

            if (args.Length != 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: dotnet DistanceRules [source-dns-file.pcap]");
                return;
            }
            var inputFilename = args[0];
            ConfigureLog(Path.ChangeExtension(inputFilename, "log"));
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
            session.Fire();
            Console.WriteLine($"done [{sw.Elapsed}].");

        }

        private static PacketModel packetCreator(string protocols, IDictionary<string, object> fields)
        {
            if (protocols.Contains("dns"))
            {
                return new DnsModel(fields);
            }
            return new GenericPacketModel(fields);
        }
    }
}
