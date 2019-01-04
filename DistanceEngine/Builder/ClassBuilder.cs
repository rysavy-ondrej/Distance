using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Distance.Utils;

namespace Distance.Engine.Builder
{
    public abstract class ClassBuilder
    {
        public abstract CodeTypeDeclaration TypeDeclaration { get; }

        protected CodeMemberProperty EmitPropertyDefinition(DiagnosticSpecification.Field field)
        {
            var property = new CodeMemberProperty
            {
                Name = field.FieldName.ToCamelCase(),
                Attributes = MemberAttributes.Public,
                Type = new CodeTypeReference(field.FieldType.ToCamelCase())
            };
            var attrib = new CodeAttributeDeclaration(new CodeTypeReference("FieldName"), new CodeAttributeArgument(new CodePrimitiveExpression(field.FieldName)));
            property.CustomAttributes.Add(attrib);
            return property;
        }

        protected CodeMemberMethod EmitToStringMethod(string className, params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "ToString",
                ReturnType = new CodeTypeReference("System.String"),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };

            CodeMethodInvokeExpression FormatString(DiagnosticSpecification.Field field)
            {
                return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("String"), "Format",
                                               new CodePrimitiveExpression($"{field.FieldName}={{0}}"),
                                               new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), field.FieldName.ToCamelCase())
                );
            }

            var elements = fields.Select(FormatString).AsEnumerable<CodeExpression>();
            var stringJoin = new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("String"), "Join",
                elements.Prepend(new CodePrimitiveExpression(' ')).ToArray());
            var addExpr = new CodeBinaryOperatorExpression(new CodePrimitiveExpression($"{className}: "), CodeBinaryOperatorType.Add, stringJoin);
            method.Statements.Add(new CodeMethodReturnStatement(addExpr));
            return method;
        }
    }
}