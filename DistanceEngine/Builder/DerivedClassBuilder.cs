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
            var className = derived.Name.ToCamelCase();

            m_typeDeclaration = new CodeTypeDeclaration(className);
            var classType = new CodeTypeReference(className);

            foreach (var field in derived.Fields)
            {
                m_typeDeclaration.Members.Add(EmitFieldDeclaration(field));
                m_typeDeclaration.Members.Add(EmitPropertyDeclaration(field));
            }

            m_typeDeclaration.Members.Add(EmitToStringMethodCode(classType, derived.Fields.ToArray()));
            m_typeDeclaration.Members.Add(EmitGetHashCodeMethodCode(classType, derived.Fields.ToArray()));
            m_typeDeclaration.Members.Add(EmitEqualsMethodCode(classType, derived.Fields.ToArray()));
        }   
    }
}
