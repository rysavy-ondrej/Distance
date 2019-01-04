using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distance.Engine.Builder
{
    public class ModuleBuilder
    {
        private DiagnosticSpecification.Module m_module;
        private CodeCompileUnit m_compileUnit;
        private CodeNamespace m_codeNamespace;

        public ModuleBuilder(DiagnosticSpecification.Module module)
        {
            this.m_module = module;
            m_compileUnit = new CodeCompileUnit();
            m_codeNamespace = new CodeNamespace(m_module.Meta.Namespace);
            m_compileUnit.Namespaces.Add(m_codeNamespace);

            m_codeNamespace.Imports.Add(new CodeNamespaceImport("Distance.Runtime"));
            m_codeNamespace.Imports.Add(new CodeNamespaceImport("Distance.Utils"));
            m_codeNamespace.Imports.Add(new CodeNamespaceImport("System"));

            foreach (var fact in m_module.Facts)
            {
                var builder = new FactClassBuilder(fact);
                m_codeNamespace.Types.Add(builder.TypeDeclaration);
            }
            foreach(var derived in m_module.Derived)
            {
                var builder = new DerivedClassBuilder(derived);
                m_codeNamespace.Types.Add(builder.TypeDeclaration);
            }
            
        }

        public CodeCompileUnit CompileUnit => m_compileUnit;
    }
}
