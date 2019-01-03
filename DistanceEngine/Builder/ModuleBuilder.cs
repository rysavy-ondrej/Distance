using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distance.Engine.Builder
{
    public class ModuleBuilder
    {
        private DiagnosticSpecification.Module module;

        public ModuleBuilder(DiagnosticSpecification.Module module)
        {
            this.module = module;
        }

        public void EmitPrologue(IndentedTextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using Distance.Runtime;");
            writer.WriteLine("using Distance.Utils;");
            writer.WriteLine($"namespace {module.Meta.Namespace}");
            writer.WriteLine("{");
            writer.Indent += 1;
        }

        public void EmitEpilogue(IndentedTextWriter writer)
        {
            writer.Indent -= 1;
            writer.WriteLine("}");
        }

        public void EmitFactClass(IndentedTextWriter writer, DiagnosticSpecification.Fact fact)
        {
            var builder = new FactClassBuilder(fact);
            builder.Emit(writer);
        }
        public void EmitDerivedClass(IndentedTextWriter writer, DiagnosticSpecification.Derived derived)
        {
            var builder = new DerivedBuilder(derived);
            builder.Emit(writer);
        }

        public void Emit(IndentedTextWriter writer)
        {
            EmitPrologue(writer);
            foreach (var fact in module.Facts) EmitFactClass(writer, fact);
            foreach (var derived in module.Derived) EmitDerivedClass(writer, derived);
            EmitEpilogue(writer);
        }
    }
}
