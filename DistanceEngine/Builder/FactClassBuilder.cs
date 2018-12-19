using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Distance.Utils;
namespace Distance.Engine.Builder
{
    public class FactClassBuilder
    {
        DiagnosticSpecification.Fact m_fact;

        public FactClassBuilder(DiagnosticSpecification.Fact fact)
        {
            m_fact = fact;
        }
        public override string ToString()
        {
            return GetClassAsString(m_fact.Name, m_fact.Where, GetFields(m_fact));
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

        private List<(string Type, string FieldName, string PropName)> GetFields(DiagnosticSpecification.Fact fact)
        {
            return fact.Select.Select(ParseFieldDeclaration).ToList();
        }

        private string GetClassAsString(string className, string filter, IEnumerable<(string Type, string FieldName, string PropName)> fieldList)
        {
            var propertyDefinitions = fieldList.Select(x => $"    public {x.Type} {x.PropName} {{ get; set; }}");
            var propertyAttributes = fieldList.Select(x => $"    [FieldName(\"{x.FieldName}\")]");
            var fieldsStringArray = String.Join(',', fieldList.Select(x => $"\"{x.FieldName}\""));

            var sb = new StringBuilder();
            sb.AppendLine($"public class {className} {{");
            sb.AppendLine($"    public static string Filter = \"{filter}\";");
            sb.AppendLine($"    public static string[] Fields = {{ {fieldsStringArray} }};");
            sb.AppendLine();
            sb.AppendLine(String.Join('\n', propertyAttributes.Zip(propertyDefinitions, (x, y) => $"{x}\n{y}\n")));
            sb.AppendLine($"    public static {className} Create(string []values) {{");
            sb.AppendLine($"        return new {className} {{");
            sb.AppendLine(String.Join(",\n", fieldList.Select((x, i) => $"             {x.PropName} = values[{i}].To{x.Type.ToCamelCase()}()")));
            sb.AppendLine($"        }};");
            sb.AppendLine("     }");
            sb.AppendLine("}");
            var classDefinition = sb.ToString();
            return classDefinition;
        }
    }
}
