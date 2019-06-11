using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Distance.Utils;

namespace Distance.Engine.Builder
{
    public abstract class ClassBuilder
    {
        public string ClassName { get; private set; }
        private readonly CodeTypeDeclaration m_typeDeclaration;
        private readonly CodeTypeReference m_typeReference;

        public CodeTypeDeclaration TypeDeclaration => m_typeDeclaration;
        public CodeTypeReference TypeReference => m_typeReference;

        protected ClassBuilder(string name)
        {
            this.ClassName = name.ToCamelCase();
            this.m_typeDeclaration = new CodeTypeDeclaration(ClassName);
            this.m_typeReference = new CodeTypeReference(ClassName);
        }

        

        protected string GetBackingFieldName(DiagnosticSpecification.Field field)
        {
            return $"_{field.FieldName.ToCamelCase()}";
        }

        protected CodeMemberProperty EmitPropertyDeclaration(DiagnosticSpecification.Field field)
        {
            var propertyName = field.FieldName.ToCamelCase();
            var backingFieldName = GetBackingFieldName(field);
            var property = new CodeMemberProperty
            {
                Name = propertyName,
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(field.FieldType.ToCamelCase())
            };

            var propertyAttributes = new CodeAttributeDeclaration(new CodeTypeReference("FieldName"), new CodeAttributeArgument(new CodePrimitiveExpression(field.FieldName)));

            property.CustomAttributes.Add(propertyAttributes);

            property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingFieldName), new CodeVariableReferenceExpression("value")));
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), backingFieldName)));

            return property;
        }

        protected CodeMemberField EmitFieldDeclaration(DiagnosticSpecification.Field field)
        {
            return new CodeMemberField
            {
                Name = GetBackingFieldName(field),
                Attributes = MemberAttributes.Private,
                Type = new CodeTypeReference(field.FieldType.ToCamelCase())
            };
        }

        /// <summary>
        /// Emits code for ToString() method.
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// <remarks>
        /// public override ToString()
        /// {
        ///    return String.Format("ClassNAME: field0={0} ... fieldn={N}", this.Prop0, ... this.PropN);
        /// }
        /// </remarks>
        protected CodeMemberMethod EmitToStringMethodCode(CodeTypeReference classType, params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "ToString",
                ReturnType = new CodeTypeReference("System.String"),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };

            var formatString = $"{classType.BaseType}: " + String.Join(' ', fields.Select((f, i) => $"{f.FieldName}={{{i}}}"));
            var formatStringArguments = fields.Select(GetPropertyReferenceExpression).AsEnumerable<CodeExpression>();

            method.Statements.Add(
                new CodeMethodReturnStatement
                {
                    Expression = new CodeMethodInvokeExpression
                                 (
                                    new CodeMethodReferenceExpression
                                    {
                                        TargetObject = new CodeTypeReferenceExpression(typeof(String)),
                                        MethodName = "Format"
                                    },
                                    formatStringArguments.Prepend(new CodePrimitiveExpression(formatString)).ToArray()
                                 )
                });

            return method;
        }

        protected CodeMemberMethod EmitGetHashCodeMethodCode(CodeTypeReference classType, params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "GetHashCode",
                ReturnType = new CodeTypeReference(typeof(int)),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };

            method.Statements.Add(
                new CodeMethodReturnStatement
                {
                    Expression = new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression
                        {
                            TargetObject = new CodeTypeReferenceExpression(typeof(HashFunction)),
                            MethodName = "GetHashCode",
                        },
                        fields.Select(GetPropertyReferenceExpression).AsEnumerable<CodeExpression>().ToArray())
                });

            return method;
        }

        private CodeExpression GetPropertyReferenceExpression(DiagnosticSpecification.Field arg)
        {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), arg.FieldName.ToCamelCase());
        }

        protected CodeMemberMethod EmitEqualsMethodCode(CodeTypeReference classType, params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "Equals",
                ReturnType = new CodeTypeReference(typeof(bool)),                
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };
            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)), "obj"));

            var castExpr = new CodeVariableDeclarationStatement
            {
                Type = classType,
                Name = "that",
                InitExpression = new CodeSnippetExpression($"obj as {classType.BaseType}")
            };

            method.Statements.Add(castExpr);

            var equalsExpressions = fields.Select(field => GetEqualsExpression(new CodeThisReferenceExpression(), new CodeVariableReferenceExpression("that"), field));

            var thatNotNullExpression = new CodeBinaryOperatorExpression
            {
                Left = new CodeVariableReferenceExpression("that"),
                Operator = CodeBinaryOperatorType.IdentityInequality,
                Right = new CodePrimitiveExpression(null)
            };

            var testExpression = equalsExpressions.Prepend(thatNotNullExpression).Aggregate((left, right) => new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right));

            method.Statements.Add(new CodeMethodReturnStatement(testExpression));
            return method;
        }

        private CodeExpression GetEqualsExpression(CodeThisReferenceExpression codeThis, CodeVariableReferenceExpression codeThat, DiagnosticSpecification.Field field)
        {
            return new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(object)), "Equals"),
                    new CodePropertyReferenceExpression(codeThis, field.FieldName.ToCamelCase()),
                    new CodePropertyReferenceExpression(codeThat, field.FieldName.ToCamelCase())
                );
        }

        protected CodeExpression CallToStringExpression(CodeExpression arg)
        {
            return new CodeMethodInvokeExpression
                (
                    new CodeMethodReferenceExpression
                    {
                        TargetObject = new CodeTypeReferenceExpression(typeof(StringUtils)),
                        MethodName = nameof(StringUtils.ToString)
                    },
                    arg
                );
        }

        protected Func<(Location, string),CodeExpression> GetReferenceExpression(IList<DiagnosticSpecification.Field> fields)
        {
            return locpath =>
            {
                var reference = locpath.Item2.Trim('{', '}');
                // TODO: analyze the rest of the path: we need access to type declarations...            
                var field = fields.FirstOrDefault(f => reference.StartsWith(f.FieldName));
                if (field == null) throw new BuildException(locpath.Item1,$"Field '{reference}' is not defined for the given object.", null);
                return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), field.FieldName.ToCamelCase());
            };
        }
    }
}