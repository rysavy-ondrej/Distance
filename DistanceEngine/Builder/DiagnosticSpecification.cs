using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Distance.Engine.Builder
{
    public static class DiagnosticSpecification
    {

        public enum Severity { Debug, Information, Warning, Error }


        public class SyntaxNode
        {
            public Location Location { get; set; }
        }

        public static Module DeserializeDocument(string path)
        {
            var input = new StreamReader(path);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeConverter(new FieldTypeConverter())
                .WithTypeConverter(new SeverityEnumConverter())
                .WithTypeConverter(new LambdaExpressionConverter())
                .Build();

            var module = deserializer.Deserialize<Module>(input);


            return module;
        }

        public class Module : SyntaxNode
        {
            List<Fact> _facts = new List<Fact>();
            List<Derived> _derived = new List<Derived>();
            List<Event> _events = new List<Event>();
            List<Rule> _rules = new List<Rule>();

            public Meta Meta { get; set; }
            public List<Fact> Facts
            {
                get => _facts;
                set { _facts.AddRange(value); }
            }
            public List<Derived> Derived
            {
                get => _derived;
                set { _derived.AddRange(value); }
            }
            public List<Event> Events
            {
                get => _events;
                set { _events.AddRange(value); }
            }
            public List<Rule> Rules
            {
                get => _rules;
                set { _rules.AddRange(value); }
            }
        }

        public class Meta : SyntaxNode
        {
            public string Namespace { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"Meta: Namespace={Namespace}";
            }
        }

        public class Fact : SyntaxNode
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Where { get; set; }
            public List<Field> Select { get; set; }
            public override string ToString()
            {
                return $"Fact: Name={Name}";
            }
        }

        public class Derived : SyntaxNode
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<Field> Fields { get; set; }
            public override string ToString()
            {
                return $"Derived: Name={Name}";
            }
        }

        public class Event : SyntaxNode
        {
            public string Name { get; set; }
            public Severity Severity { get; set; }
            public string Description { get; set; }
            public string Message { get; set; }
            public List<Field> Fields { get; set; }
            public override string ToString()
            {
                return $"Event: Name={Name}";
            }
        }

        public class Field : SyntaxNode
        {
            public string FieldType { get; set; }
            public string FieldName { get; set; }
            public override string ToString()
            {
                return $"Field: Name={FieldName} Type={FieldType}";
            }
        }

        public class Rule : SyntaxNode
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public When When { get; set; }
            public Then Then { get; set; }
            public override string ToString()
            {
                return $"Rule Name={Name}";
            }
        }
        public class When : SyntaxNode
        {
            public List<Expression> Match { get; set; }
            public List<Expression> Not { get; set; }
        }
        public class Then : SyntaxNode
        {
            public string Yield { get; set; }
            public string Error { get; set; }
            public string Warn { get; set; }
            public string Info { get; set; }
        }


        internal class LambdaExpressionConverter : IYamlTypeConverter
        {
            private static readonly Type m_lambdaExpressionType = typeof(Expression);
            public bool Accepts(Type type)
            {
                return type == m_lambdaExpressionType;
            }

            public object ReadYaml(IParser parser, Type type)
            {
                parser.MoveNext();
                return null;
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                throw new NotImplementedException();
            }
        }

        internal class SeverityEnumConverter : IYamlTypeConverter
        {

            private static readonly Type m_severityType = typeof(Severity);
            public bool Accepts(Type type)
            {
                return type == m_severityType;
            }

            public object ReadYaml(IParser parser, Type type)
            {
                try
                {
                    var scalar = parser.Current as Scalar;
                    if (!Accepts(type) || scalar == null)
                    {
                        throw new InvalidDataException("Invalid YAML content.");
                    }
                    
                    parser.MoveNext();
                    if (Enum.TryParse<Severity>(scalar.Value, true, out var result))
                    {
                        return result;
                    }
                    else
                    {
                        throw new YamlDotNet.Core.SyntaxErrorException(parser.Current.Start, parser.Current.End, $"{scalar.Value} is not a valid severity value");
                    }
                }
                catch (Exception e)
                {
                    throw new YamlDotNet.Core.SyntaxErrorException(parser.Current.Start, parser.Current.End, e.Message);
                }
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                throw new NotImplementedException();
            }
        }

        internal class FieldTypeConverter : IYamlTypeConverter
        {
            private static readonly Type m_fieldType = typeof(Field);
            public bool Accepts(Type type)
            {
                return type == m_fieldType;
            }

            public object ReadYaml(IParser parser, Type type)
            {
                try
                {
                    var scalar = parser.Current as Scalar;                    
                    if (!Accepts(type) || scalar == null)
                    {
                        throw new InvalidDataException("Invalid YAML content.");
                    }
                    var location = new Location("", parser.Current.Start, parser.Current.End);
                    parser.MoveNext();
                    return ParseFieldDeclaration(location, scalar.Value);
                }
                catch(Exception e)
                {
                    throw new YamlDotNet.Core.SyntaxErrorException(parser.Current.Start, parser.Current.End, e.Message);
                }
            }

            public void WriteYaml(IEmitter emitter, object value, Type type)
            {
                throw new NotImplementedException();
            }

            private static Field ParseFieldDeclaration(Location location, string declaration)
            {
                var identifier = @"[_a-zA-Z][_\.a-zA-Z0-9]*(\[\])?";
                var pattern = $"(?<FieldType>{identifier})\\s+(?<FieldName>{identifier})";
                var match = Regex.Match(declaration, pattern);
                if (match.Success)
                {
                    var type = match.Groups["FieldType"].Value;
                    var name = match.Groups["FieldName"].Value;
                    return new Field { FieldName = name, FieldType = type, Location = location };
                }
                else
                {
                    throw new YamlDotNet.Core.SyntaxErrorException(location.Start, location.End, $"Syntax error in field declaration '{declaration}'.");
                }
            }
        }
    }


}