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
        public abstract CodeTypeDeclaration TypeDeclaration { get; }

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
            var attrib = new CodeAttributeDeclaration(new CodeTypeReference("FieldName"), new CodeAttributeArgument(new CodePrimitiveExpression(field.FieldName)));
            property.CustomAttributes.Add(attrib);
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

        protected CodeMemberMethod EmitToStringMethodCode(CodeTypeReference classType, params DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "ToString",
                ReturnType = new CodeTypeReference("System.String"),
                Attributes = MemberAttributes.Override | MemberAttributes.Public,
            };

            var formatString = $"{classType.BaseType}: " + String.Join(' ', fields.Select((f, i) => $"{f.FieldName}={{{i}}}"));
            var fieldExpressions = fields.Select(GetPropertyReferenceExpression).AsEnumerable<CodeExpression>();

            var formatStringExpression = new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(String)), "Format"),
                        fieldExpressions.Prepend(new CodePrimitiveExpression(formatString)).ToArray()
                        );

            method.Statements.Add(new CodeMethodReturnStatement(formatStringExpression));
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
            
            var fieldExpressions = fields.Select(GetPropertyReferenceExpression).AsEnumerable<CodeExpression>();
            method.Statements.Add(
                new CodeMethodReturnStatement(
                    new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(HashFunction)), "GetHashCode"), 
                        fieldExpressions.ToArray())));
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

            var castExpr = new CodeVariableDeclarationStatement(classType, "that",
                // new CodeCastExpression(classType, new CodeVariableReferenceExpression("obj")));
                new CodeSnippetExpression($"obj as {classType.BaseType}"));
            method.Statements.Add(castExpr);

            var equalsExpressions = fields.Select(field => GetEqualsExpression(new CodeThisReferenceExpression(), new CodeVariableReferenceExpression("that"), field));
            var thatNotNullExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("that"), CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
            var testExpression = equalsExpressions.Prepend(thatNotNullExpression).Aggregate((left, right) => new CodeBinaryOperatorExpression(left, CodeBinaryOperatorType.BooleanAnd, right));

            method.Statements.Add(new CodeMethodReturnStatement(testExpression));
            return method;
        }

        private CodeExpression GetEqualsExpression(CodeThisReferenceExpression codeThis, CodeVariableReferenceExpression codeThat, DiagnosticSpecification.Field field)
        {
            return new CodeMethodInvokeExpression(
                 new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(object)), "Equals"),
                 new CodePropertyReferenceExpression(codeThis, field.FieldName.ToCamelCase()),
                 new CodePropertyReferenceExpression(codeThat, field.FieldName.ToCamelCase()));                 
        }
    }
}