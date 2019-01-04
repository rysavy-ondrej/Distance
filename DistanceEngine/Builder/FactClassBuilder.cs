using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Distance.Utils;
namespace Distance.Engine.Builder
{
    public class FactClassBuilder : ClassBuilder
    {
        DiagnosticSpecification.Fact m_fact;
        CodeTypeDeclaration m_typeDeclaration;

        public override CodeTypeDeclaration TypeDeclaration => m_typeDeclaration;

        public FactClassBuilder(DiagnosticSpecification.Fact fact)
        {
            m_fact = fact;
            m_typeDeclaration = new CodeTypeDeclaration(fact.Name.ToCamelCase());
        }
        public override string ToString()
        {
            return $"FactClassBuilder: Fact={{{m_fact}}}";
        }


        void EmitPropertyDefinition(IndentedTextWriter writer, DiagnosticSpecification.Field field)
        {
            writer.WriteLine($"[FieldName(\"{field.FieldName}\")]");
            writer.WriteLine($"public {field.FieldType} {field.FieldName.ToCamelCase()} {{ get; set; }}");
        }

        void EmitToStringMethod(IndentedTextWriter writer)
        {
            writer.WriteLine("public override string ToString()");
            writer.WriteLine("{"); writer.Indent += 1;
            var fieldStr = String.Join(' ', m_fact.Select.Select(f => $"{f.FieldName}={{{f.FieldName.ToCamelCase()}}}"));
            writer.WriteLine($"return $\"{m_fact.Name}: {fieldStr}\";");
            writer.Indent -= 1; writer.WriteLine("}");
        }

        void EmitGetHashCodeMethod(IndentedTextWriter writer)
        {
            var properties = String.Join(",", m_fact.Select.Select(f => f.FieldName.ToCamelCase()));
            writer.WriteLine($"public override int GetHashCode() => HashFunction.GetHashCode({properties});");
        }

        void EmitEqualsMethod(IndentedTextWriter writer)
        {
            var properties = String.Join(" && ", m_fact.Select.Select(f => $"Equals(this.{f.FieldName.ToCamelCase()}, that.{f.FieldName.ToCamelCase()})"));
            writer.WriteLine("public override bool Equals(object obj)");
            writer.WriteLine("{"); writer.Indent += 1;
            writer.WriteLine($"return (obj is {m_fact.Name.ToCamelCase()} that) && {properties};");
            writer.Indent -= 1; writer.WriteLine("}");
        }

        void EmitCreateMethod(IndentedTextWriter writer)
        {
            writer.WriteLine($"public static {m_fact.Name.ToCamelCase()} Create(string[] values)");
            writer.WriteLine("{"); writer.Indent += 1;
            writer.WriteLine($"return new {m_fact.Name.ToCamelCase()}");
            writer.WriteLine("{"); writer.Indent += 1;
            for (int i =0; i < m_fact.Select.Count; i++)
            {
                var field = m_fact.Select[i];
                writer.WriteLine($"{field.FieldName.ToCamelCase()} = values[{i}].To{field.FieldType.ToCamelCase()}(),");
            }
            writer.Indent -= 1; writer.WriteLine("};");
            writer.Indent -= 1; writer.WriteLine("}");
        }

        public  void Emit(IndentedTextWriter writer)
        {
            var className = m_fact.Name.ToCamelCase();
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{"); writer.Indent += 1;

            var fieldsStringArray = String.Join(',', m_fact.Select.Select(x => $"\"{x.FieldName}\""));
            writer.WriteLine($"public static string Filter = \"{m_fact.Where}\";");
            writer.WriteLine($"public static string[] Fields = {{ {fieldsStringArray} }};");

            foreach (var field in m_fact.Select)
                EmitPropertyDefinition(writer, field);
            EmitEqualsMethod(writer);
            EmitGetHashCodeMethod(writer);
            EmitToStringMethod(writer);
            EmitCreateMethod(writer);
            writer.Indent -= 1; writer.WriteLine("}");
        }
    }
}
