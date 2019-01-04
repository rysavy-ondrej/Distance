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
        CodeTypeDeclaration m_typeDeclaration;

        public override CodeTypeDeclaration TypeDeclaration => m_typeDeclaration;

        public FactClassBuilder(DiagnosticSpecification.Fact fact)
        {
            m_fact = fact;
            var className = fact.Name.ToCamelCase();
            m_typeDeclaration = new CodeTypeDeclaration(className);
            foreach (var field in fact.Select)
            {
                m_typeDeclaration.Members.Add(EmitFieldDeclaration(field));
                m_typeDeclaration.Members.Add(EmitPropertyDeclaration(field));
            }
            var typeReference = new CodeTypeReference(className);

            m_typeDeclaration.Members.Add(EmitToStringMethodCode(typeReference, fact.Select.ToArray()));
            m_typeDeclaration.Members.Add(EmitGetHashCodeMethodCode(typeReference, fact.Select.ToArray()));
            m_typeDeclaration.Members.Add(EmitEqualsMethodCode(typeReference, fact.Select.ToArray()));
            m_typeDeclaration.Members.Add(EmitFilterFieldDeclaration(fact.Where));
            m_typeDeclaration.Members.Add(EmitFieldsFieldDeclaration(fact.Select.ToArray()));
            m_typeDeclaration.Members.Add(EmitCreateMethodCode(typeReference, fact.Select.ToArray()));
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
                new CodeAssignStatement(
                    new CodeFieldReferenceExpression(new CodeVariableReferenceExpression("newObj"), GetBackingFieldName(f)),
                    new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(
                        new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("values"), new CodePrimitiveExpression(i)),
                        $"To{f.FieldType.ToCamelCase()}"))
                ));

            method.Statements.AddRange(assignExpressions.ToArray());
            method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("newObj")));
            return method;
        }
    }
}
