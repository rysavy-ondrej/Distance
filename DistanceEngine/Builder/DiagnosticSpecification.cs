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
        public static Module DeserializeDocument(string path)
        {
            var input = new StreamReader(path);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .WithTypeConverter(new FieldTypeConverter())
                .WithTypeConverter(new LambdaExpressionConverter())
                .Build();

            var module = deserializer.Deserialize<Module>(input);


            return module;
        }

        public class Module
        {
            public Meta Meta { get; set; }
            public List<Fact> Facts { get; set; }
            public List<Rule> Rules { get; set; }
        }

        public class Meta
        {
            public string Namespace { get; set; }
            public string Description { get; set; }
        }

        public class Fact
        {
            public string Name { get; set; }
            public string Where { get; set; }
            public List<Field> Select { get; set; }
        }

        public class Field
        {
            public Type FieldType { get; set; }
            public string FieldName { get; set; }
        }

        public class Rule
        {
            public string Name { get; set; }
            public When When { get; set; }
            public Then Then { get; set; }
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
                
                return null;
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

            enum FieldType { Bool, Int, Long, Float, Double, String };
            private static Type GetFieldType(FieldType ftype)
            {
                switch (ftype)
                {
                    case FieldType.Bool: return typeof(bool);
                    case FieldType.Float: return typeof(float);                    
                    case FieldType.Double: return typeof(double);
                    case FieldType.Int: return typeof(int);
                    case FieldType.Long: return typeof(long);
                    case FieldType.String: return typeof(string);
                }
                throw new ArgumentException("Invalid type specified.");
            }

            private static Field ParseFieldDeclaration(string declaration)
            {
                var ident = @"[_a-zA-Z][_\.a-zA-Z0-9]*";
                var pattern = $"({ident})\\s+({ident})";
                var m = Regex.Match(declaration, pattern);
                if (m.Success)
                {
                    var type = m.Groups[1].Value;
                    var name = m.Groups[2].Value;
                    if (Enum.TryParse<FieldType>(type, true, out var ftype))
                    {
                        return new Field { FieldName = name, FieldType = GetFieldType(ftype) };
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid or unknown type specified: '{type}'.");
                    }
                }
                else
                {
                    throw new ArgumentException($"Syntax error in field declaration '{declaration}'.");
                }
            }
        }
    }
}