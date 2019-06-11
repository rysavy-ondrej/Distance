﻿using Distance.Runtime;
using Distance.Utils;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Distance.Engine.Builder
{
    class EventClassBuilder : ClassBuilder
    {
        private readonly DiagnosticSpecification.Event m_event;
        public EventClassBuilder(DiagnosticSpecification.Event @event) : base(@event.Name)
        {
            m_event = @event;
            TypeDeclaration.BaseTypes.Add(typeof(Distance.Runtime.DistanceEvent));

            foreach (var field in @event.Fields)
            {
                TypeDeclaration.Members.Add(EmitFieldDeclaration(field));
                TypeDeclaration.Members.Add(EmitPropertyDeclaration(field));
            }
            TypeDeclaration.Members.Add(EmitToStringMethodCode(TypeReference, @event.Fields.ToArray()));
            TypeDeclaration.Members.Add(EmitGetHashCodeMethodCode(TypeReference, @event.Fields.ToArray()));
            TypeDeclaration.Members.Add(EmitEqualsMethodCode(TypeReference, @event.Fields.ToArray()));
            TypeDeclaration.Members.Add(EmitNamePropertyDeclaration());
            TypeDeclaration.Members.Add(EmitMessagePropertyDeclaration());
            TypeDeclaration.Members.Add(EmitSeverityPropertyDeclaration());
        }


        CodeMemberProperty EmitNamePropertyDeclaration()
        {
            var property = new CodeMemberProperty
            {
                Name = "Name",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(m_event.Name)));
            return property;
        }
        CodeMemberProperty EmitMessagePropertyDeclaration()
        {
            var property = new CodeMemberProperty
            {
                Name = "Message",
                Type = new CodeTypeReference(typeof(string)),
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };

            var propertyReferences = new List<(Location,string)>();
            string Matcher(Match match)
            {
                propertyReferences.Add((Location.Empty, match.Value));
                return $"{{{propertyReferences.Count - 1}}}";
            }

            var formatStringArguments = propertyReferences.Select(GetReferenceExpression(m_event.Fields)).Select(CallToStringExpression);

            var formatString = Regex.Replace(m_event.Message, @"{[^}]+}", new MatchEvaluator(Matcher));


            var formatMethod = new CodeMethodInvokeExpression
                                 (
                                    new CodeMethodReferenceExpression
                                    {
                                        TargetObject = new CodeTypeReferenceExpression(typeof(String)),
                                        MethodName = nameof(String.Format)
                                    },
                                    formatStringArguments.Prepend(new CodePrimitiveExpression(formatString)).ToArray()
                                 );


            property.GetStatements.Add(new CodeMethodReturnStatement(formatMethod));
            return property;
        }

        CodeMemberProperty EmitSeverityPropertyDeclaration()
        {
            var property = new CodeMemberProperty
            {
                Name = "Severity",
                Type = new CodeTypeReference(typeof(EventSeverity)),
                Attributes = MemberAttributes.Public | MemberAttributes.Override
            };

            var severityValue = typeof(EventSeverity).FullName + "." + m_event.Severity.ToString();
            property.GetStatements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression(severityValue)));
            return property;
        }
    }
}
