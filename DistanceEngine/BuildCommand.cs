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
using Microsoft.CSharp;
using System.CodeDom;

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
                var inputPath = sourceProject.Value;
                var outputPath = Path.ChangeExtension(inputPath, "gen.cs");
                var module = DiagnosticSpecification.DeserializeDocument(inputPath);
                var moduleBuilder = new ModuleBuilder(module);
                GenerateCSharpCode(moduleBuilder.CompileUnit, outputPath);      
                return 0;
            });
        }

        public static string GenerateCSharpCode(CodeCompileUnit compileunit, string outpufile)
        {
            // Generate the code with the C# code provider.
            var provider = new CSharpCodeProvider();

            // Create a TextWriter to a StreamWriter to the output file.
            using (var sw = new StreamWriter(outpufile, false))
            {
                var tw = new IndentedTextWriter(sw, "    ");

                // Generate source code using the code provider.
                provider.GenerateCodeFromCompileUnit(compileunit, tw,
                    new CodeGeneratorOptions());

                // Close the output file.
                tw.Close();
            }

            return outpufile;
        }
    }
}