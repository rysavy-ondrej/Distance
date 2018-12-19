using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Distance.Engine.Program;

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
                var module = DiagnosticSpecification.DeserializeDocument(sourceProject.Value);

                // generate fact classes:

                foreach (var fact in module.Facts)
                {
                    CreateFactClass(fact);
                }

                return 0;
            });
        }

        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

        private string GetCSharpName(string fieldName)
        {
            var name2 = myTI.ToTitleCase(fieldName).Replace(".","");
            return name2;
        }

        private void CreateFactClass(DiagnosticSpecification.Fact fact)
        {
            var sb = new StringBuilder();
            var className = fact.Name;
            sb.AppendLine($"public class {className} {{");

            var fieldList = fact.Select.Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            var propDefs = fieldList.Select(x => $"    public {x[0]} {GetCSharpName(x[1])} {{ get; set; }}");
            var metaDefs = fieldList.Select(x => $"    [FieldName(\"{x[1]}\")]");
            var fieldStr = String.Join(',', fieldList.Select(x => $"\"{x[1]}\""));

            sb.AppendLine($"    public static string Filter = \"{fact.Where}\";");
            sb.AppendLine($"    public static string[] Fields = {{ {fieldStr} }};");            
            sb.AppendLine();
            sb.AppendLine(String.Join('\n', metaDefs.Zip(propDefs, (x, y) => $"{x}\n{y}\n")));

            

            sb.AppendLine($"    public static {className} Create(string []values) {{");
            sb.AppendLine($"        return new {className} {{");
            sb.AppendLine(String.Join(",\n", fieldList.Select((x, i) => $"             {GetCSharpName(x[1])} = values[{i}].To{GetCSharpName(x[0])}()")));
            sb.AppendLine($"        }};");
            sb.AppendLine("     }");
            sb.AppendLine("}");
            var classDefinition = sb.ToString();
        }
    }
}