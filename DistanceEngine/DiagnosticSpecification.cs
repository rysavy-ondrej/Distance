using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Distance.Engine
{
    public static class DiagnosticSpecification
    {
        public static Module DeserializeDocument(string path)
        {
            var input = new StreamReader(path);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
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
            public List<string> Select { get; set; }
        }

        public class Rule
        {
            public string Name { get; set; }
            public When When { get; set; }
            public Then Then { get; set; }
        }
        public class When
        {
            public List<string> Match { get; set; }
            public List<string> Not { get; set; }
        }
        public class Then
        {
            public string Yield { get; set; }
            public string Error { get; set; }
            public string Warn { get; set; }
            public string Info { get; set; }
        }
    }
}