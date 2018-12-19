using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Distance.Utils;
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

        private (string Type, string FieldName, string PropName) ParseFieldDeclaration(string declaration)
        {
            var ident = @"[_a-zA-Z][_\.a-zA-Z0-9]*";
            var m = Regex.Match(declaration, $"({ident})\\s+({ident})");
            if (m.Success)
            {
                var type = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                return (Type: type, FieldName: name, PropName: name.ToCamelCase());
            }
            else
            {
                throw new YamlDotNet.Core.SyntaxErrorException($"Field declaration '{declaration}' has bad format.");
            }
        }
        private void CreateFactClass(DiagnosticSpecification.Fact fact)
        {
            var sb = new StringBuilder();
            var className = fact.Name;
            sb.AppendLine($"public class {className} {{");

            var fieldList = fact.Select.Select(ParseFieldDeclaration);

            var propDefs = fieldList.Select(x => $"    public {x.Type} {x.PropName} {{ get; set; }}");
            var metaDefs = fieldList.Select(x => $"    [FieldName(\"{x.FieldName}\")]");
            var fieldStr = String.Join(',', fieldList.Select(x => $"\"{x.FieldName}\""));

            sb.AppendLine($"    public static string Filter = \"{fact.Where}\";");
            sb.AppendLine($"    public static string[] Fields = {{ {fieldStr} }};");            
            sb.AppendLine();
            sb.AppendLine(String.Join('\n', metaDefs.Zip(propDefs, (x, y) => $"{x}\n{y}\n")));

            

            sb.AppendLine($"    public static {className} Create(string []values) {{");
            sb.AppendLine($"        return new {className} {{");
            sb.AppendLine(String.Join(",\n", fieldList.Select((x, i) => $"             {x.PropName} = values[{i}].To{x.Type.ToCamelCase()}()")));
            sb.AppendLine($"        }};");
            sb.AppendLine("     }");
            sb.AppendLine("}");
            var classDefinition = sb.ToString();
        }
    }
}