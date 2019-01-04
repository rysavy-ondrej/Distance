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

        public override CodeTypeDeclaration TypeDeclaration => m_typeDeclaration;

        public DerivedClassBuilder(DiagnosticSpecification.Derived derived)
        {
            m_derived = derived;
            m_typeDeclaration = new CodeTypeDeclaration(derived.Name.ToCamelCase());
            m_typeDeclaration.Members.Add(EmitToStringMethod(derived.Name.ToCamelCase(), derived.Fields.ToArray()));
            foreach(var field in derived.Fields)
            {
                m_typeDeclaration.Members.Add(EmitPropertyDefinition(field));
            }
        }

   

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

        public void Emit(IndentedTextWriter writer)
        {
            var className = m_derived.Name.ToCamelCase();
            writer.WriteLine($"public class {className}");
            writer.WriteLine("{"); writer.Indent += 1;

            EmitEqualsMethod(writer);
            EmitGetHashCodeMethod(writer);
            writer.Indent -= 1; writer.WriteLine("}");
        }
    }
}
