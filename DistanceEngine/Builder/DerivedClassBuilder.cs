using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Distance.Utils;
namespace Distance.Engine.Builder
{
    public class DerivedClassBuilder : ClassBuilder
    {
        private readonly DiagnosticSpecification.Derived m_derived;
        CodeTypeDeclaration m_typeDeclaration;

        public DerivedClassBuilder(DiagnosticSpecification.Derived derived)
        {
            m_derived = derived;
            m_typeDeclaration = new CodeTypeDeclaration(derived.Name.ToCamelCase());

        }

        void EmitPropertyDefinition(IndentedTextWriter writer, DiagnosticSpecification.Field field)
        {
            writer.WriteLine($"[FieldName(\"{field.FieldName}\")]");
            writer.WriteLine($"public {field.FieldType} {field.FieldName.ToCamelCase()} {{ get; set; }}");
        }

        public static CodeMemberMethod EmitToStringMethod(params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "ToString",
                ReturnType = new CodeTypeReference("System.String"),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };
            foreach (var field in fields)
            {
                var stringPrimitive = new CodePrimitiveExpression();
                new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("String"),
                                               "Format", 
                                               new CodePrimitiveExpression("{0}={1}"), 
                                               new CodePrimitiveExpression(field.FieldName),
                                               new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), field.FieldName.ToCamelCase())
                    );
            }
            method.Statements.Add(new CodeMethodReturnStatement(stringFormat));
        }
            var fieldStr = String.Join(' ', m_derived.Fields.Select(f => $"{f.FieldName}={{{f.FieldName.ToCamelCase()}}}"));
            writer.WriteLine($"return $\"{m_derived.Name}: {fieldStr}\";");


        void EmitGetHashCodeMethod(IndentedTextWriter writer)
        {
            var properties = String.Join(",", m_derived.Fields.Select(f => f.FieldName.ToCamelCase()));
            writer.WriteLine($"public override int GetHashCode() => HashFunction.GetHashCode({properties});");
        }

        void EmitEqualsMethod(IndentedTextWriter writer)
        {
            var properties = String.Join(" && ", m_derived.Fields.Select(f => $"Equals(this.{f.FieldName.ToCamelCase()}, that.{f.FieldName.ToCamelCase()})"));
            writer.WriteLine("public override bool Equals(object obj)");
            writer.WriteLine("{"); writer.Indent += 1;
            writer.WriteLine($"return (obj is {m_derived.Name.ToCamelCase()} that) && {properties};");
            writer.Indent -= 1;  writer.WriteLine("}");
        }

        public override void Emit(IndentedTextWriter writer)
        {
            var className = m_derived.Name.ToCamelCase();
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{"); writer.Indent += 1;

            foreach (var field in m_derived.Fields)
                EmitPropertyDefinition(writer, field);
            EmitEqualsMethod(writer);
            EmitGetHashCodeMethod(writer);
            EmitToStringMethod(writer);
            writer.Indent -= 1; writer.WriteLine("}");
        }
    }
}
