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
        public DerivedClassBuilder(DiagnosticSpecification.Derived derived) : base (derived.Name)
        {
            m_derived = derived;           
            foreach (var field in derived.Fields)
            {
                TypeDeclaration.Members.Add(EmitFieldDeclaration(field));
                TypeDeclaration.Members.Add(EmitPropertyDeclaration(field));
            }

            TypeDeclaration.Members.Add(EmitToStringMethodCode(TypeReference, derived.Fields.ToArray()));
            TypeDeclaration.Members.Add(EmitGetHashCodeMethodCode(TypeReference, derived.Fields.ToArray()));
            TypeDeclaration.Members.Add(EmitEqualsMethodCode(TypeReference, derived.Fields.ToArray()));
        }   
    }
}
