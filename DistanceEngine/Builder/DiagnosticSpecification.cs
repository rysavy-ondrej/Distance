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

        public class Module
        {
            public Meta Meta { get; set; }
            public List<Fact> Facts { get; set; }
            public List<Derived> Derived { get; set; }
            public List<Event> Events { get; set; }
            public List<Rule> Rules { get; set; }
        }

        public class Meta
        {
            public string Namespace { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"Meta: Namespace={Namespace}";
            }
        }

        public class Fact
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

        public class Derived
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<Field> Fields { get; set; }
            public override string ToString()
            {
                return $"Derived: Name={Name}";
            }
        }

        public class Event
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

        public class Field
        {
            public string FieldType { get; set; }
            public string FieldName { get; set; }
            public override string ToString()
            {
                return $"Field: Name={FieldName} Type={FieldType}";
            }
        }

        public class Rule
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
        public class When
        {
            public List<Expression> Match { get; set; }
            public List<Expression> Not { get; set; }
        }
        public class Then
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
                    return Enum.Parse<Severity>(scalar.Value, true);
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
                    parser.MoveNext();
                    return ParseFieldDeclaration(scalar.Value);
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

            private static Field ParseFieldDeclaration(string declaration)
            {
                var identifier = @"[_a-zA-Z][_\.a-zA-Z0-9]*";
                var pattern = $"({identifier})\\s+({identifier})";
                var match = Regex.Match(declaration, pattern);
                if (match.Success)
                {
                    var type = match.Groups[1].Value;
                    var name = match.Groups[2].Value;
                    return new Field { FieldName = name, FieldType = type };
                }
                else
                {
                    throw new ArgumentException($"Syntax error in field declaration '{declaration}'.");
                }
            }
        }
    }
}