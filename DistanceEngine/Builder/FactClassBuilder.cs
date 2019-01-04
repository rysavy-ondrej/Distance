using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Distance.Utils;
namespace Distance.Engine.Builder
{
    public class FactClassBuilder : ClassBuilder
    {
        DiagnosticSpecification.Fact m_fact;
        public FactClassBuilder(DiagnosticSpecification.Fact fact) : base (fact.Name)
        {
            m_fact = fact;
            foreach (var field in fact.Select)
            {
                TypeDeclaration.Members.Add(EmitFieldDeclaration(field));
                TypeDeclaration.Members.Add(EmitPropertyDeclaration(field));
            }
            TypeDeclaration.Members.Add(EmitToStringMethodCode(TypeReference, fact.Select.ToArray()));
            TypeDeclaration.Members.Add(EmitGetHashCodeMethodCode(TypeReference, fact.Select.ToArray()));
            TypeDeclaration.Members.Add(EmitEqualsMethodCode(TypeReference, fact.Select.ToArray()));
            TypeDeclaration.Members.Add(EmitFilterFieldDeclaration(fact.Where));
            TypeDeclaration.Members.Add(EmitFieldsFieldDeclaration(fact.Select.ToArray()));
            TypeDeclaration.Members.Add(EmitCreateMethodCode(TypeReference, fact.Select.ToArray()));
        }
        public override string ToString()
        {
            return $"FactClassBuilder: Fact={{{m_fact}}}";
        }

        CodeMemberField EmitFilterFieldDeclaration(string filter)
        {
            return new CodeMemberField
            {
                Name = "Filter",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Static | MemberAttributes.Public,
                InitExpression = new CodePrimitiveExpression(filter)
            };
        }

        CodeMemberField EmitFieldsFieldDeclaration(DiagnosticSpecification.Field[] fields)
        {
            var fieldNames = fields.Select(x => new CodePrimitiveExpression(x.FieldName));
            return new CodeMemberField
            {
                Name = "Fields",
                Type = new CodeTypeReference(typeof(string[])),
                Attributes = MemberAttributes.Static | MemberAttributes.Public,
                InitExpression = new CodeArrayCreateExpression(new CodeTypeReference(typeof(string[])), fieldNames.ToArray())
            };
        }
        CodeMemberMethod EmitCreateMethodCode(CodeTypeReference classType, DiagnosticSpecification.Field[] fields)
        {
            var method = new CodeMemberMethod
            {
                Name = "Create",
                ReturnType = classType,
                Attributes = MemberAttributes.Static | MemberAttributes.Public,
            };

            method.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string[])), "values"));

            var newObj = new CodeVariableDeclarationStatement
            {
                Name = "newObj",
                Type = classType,
                InitExpression = new CodeObjectCreateExpression(classType)
            };

            method.Statements.Add(newObj);

            var assignExpressions = fields.Select((f, i) =>
                new CodeAssignStatement
                {
                    Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("newObj"), GetBackingFieldName(f)),
                    Right = new CodeMethodInvokeExpression(
                        new CodeMethodReferenceExpression
                        {
                            TargetObject = new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("values"), new CodePrimitiveExpression(i)),
                            MethodName = $"To{f.FieldType.ToCamelCase()}"
                        })
                });

            method.Statements.AddRange(assignExpressions.ToArray());
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("newObj")));
            return method;
        }
    }
}
