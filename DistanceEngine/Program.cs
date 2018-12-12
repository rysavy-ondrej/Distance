using Distance.Domain.Dns;
using Distance.Rules;
using Distance.Rules.Dns;
using NRules;
using NRules.Fluent;
using NRules.RuleModel;
using System;
using System.IO;
using System.Linq;

namespace Distance.Engine
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length != 1 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: dotnet DistanceRules [source-dns-file.csv]");
                return;
            }
            var inputFilename = args[0];

            //Load rules
            var repository = new RuleRepository();
            var assembly = typeof(MatchRequestResponseRule).Assembly;
            Console.Write($"Loading rules from assembly '{assembly.FullName}'...");
            repository.Load(x => x.From(assembly));
            Console.WriteLine("ok.");

            //Compile rules
            Console.Write("Compiling rules...");
            var factory = repository.Compile();
            Console.WriteLine("ok.");

            //Create a working session
            Console.Write("Creating a session...");
            var session = factory.CreateSession();
            Console.WriteLine("ok.");

            //Load domain model
            Console.Write($"Loading facts from '{inputFilename}'...");
            var dns = DnsModel.LoadFromJson(inputFilename).ToList();
            Console.WriteLine("ok.");

            //Insert facts into rules engine's memory
            Console.Write("Inserting facts to the session...");
            session.InsertAll(dns);
            Console.WriteLine("ok.");

            Console.WriteLine("All done. Starting the engine:");
            //Start match/resolve/act cycle
            session.Fire();
        }
    }
}
