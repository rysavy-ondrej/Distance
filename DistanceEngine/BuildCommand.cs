using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Distance.Utils;
using static Distance.Engine.Program;
using Distance.Engine.Builder;
using System.IO;
using System.CodeDom.Compiler;

namespace Distance.Engine
{
    internal class BuildCommand
    {
        private Options commonOptions;

        public BuildCommand(Options commonOptions)
        {
            this.commonOptions = commonOptions;
        }


        public static string Name => "build";
        public void Configuration(CommandLineApplication command)
        {
            command.Description = "Build a distance ruleset from the source yaml project.";
            command.HelpOption("-?|-help");

            var sourceProject = command.Argument("SourceYamlProject",
                "A file with the source yaml ruleset project. Multiple values can be specified.",
                false);

            command.OnExecute(() =>
            {
                var inpath = sourceProject.Value;
                var outpath = Path.ChangeExtension(inpath, "gen.cs");
                var module = DiagnosticSpecification.DeserializeDocument(inpath);
                var moduleBuilder = new ModuleBuilder(module);

                using (var writer = new IndentedTextWriter(new StreamWriter(outpath)))
                {
                    moduleBuilder.Emit(writer);
                    writer.Flush();
                }                
                return 0;
            });
        }
    }
}