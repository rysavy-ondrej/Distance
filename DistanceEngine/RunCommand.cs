using Distance.Engine.Runner;
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
        public RunCommand(Program.Options options)
        {
            this.options = options;
        }

        public static string Name => "run";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Run a distance ruleset against the specified input file(s).";
            command.HelpOption("--|-help");
            var profileAssemblyOption = command.Option("-profile <PROFILENAME>", "Specifies assembly that contains a diagnostic profile.", CommandOptionType.MultipleValue);
            var parallelOption = command.Option("-parallel <NUMBER>", "Sets the degree of parallelism when loading and decoding of input data (-1 means unlimited).", CommandOptionType.SingleValue);
            var decoderClassOption = command.Option("-decoder <STRING>", "Sets the decoder to use for dissecting packets and load facts. Default is 'Shark'.", CommandOptionType.SingleValue);
            var inputFile = command.Argument("InputPcapFile", "An input packet capture file to analyze.", false);
            var dumpRete = command.Option("-dumpRete <FILENAME>", "Writes the snapshot of RETE network to specified file using GEXF fileformat.", CommandOptionType.SingleValue);
            command.OnExecute(async () =>
            {
                
                if (!profileAssemblyOption.HasValue())
                {
                        throw new CommandParsingException(command, "Required options '-profile' is missing.");
                }
                var diagnosticProfileAssemblies = profileAssemblyOption.Values.Select(x=>Assembly.LoadFrom(GetAssemblyPath(x))).ToArray();
                var analyzer = new CaptureAnalyzer(diagnosticProfileAssemblies);
                if (parallelOption.HasValue()) analyzer.DegreeOfParallelism = Int32.Parse(parallelOption.Value());
                if (dumpRete.HasValue()) analyzer.DumpReteToFile(dumpRete.Value());
                await analyzer.AnalyzeCaptureFile(inputFile.Value);
                return 0;
            });            
        }

        public static string GetAssemblyPath(string profileName)
        {
            if (!profileName.EndsWith(".dll"))
            {
                profileName += ".dll";
            }

            var fullpath = Path.GetFullPath(profileName);
            if (File.Exists(fullpath)) return Path.GetFullPath(profileName);

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

            fullpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), profileName);
            if (File.Exists(fullpath)) return fullpath;

            throw new FileNotFoundException($"Could not locate assembly file '{profileName}'.", profileName);
        }      
    }
}