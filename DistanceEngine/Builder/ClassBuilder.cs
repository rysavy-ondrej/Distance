using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Distance.Engine.Builder
{
    public abstract class ClassBuilder
    {
        public abstract CodeTypeDeclaration TypeDeclaration { get; }
    }
}