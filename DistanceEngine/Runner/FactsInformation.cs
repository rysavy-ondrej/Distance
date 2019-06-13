using System;

namespace Distance.Engine.Runner
{
    public class FactsInformation
    {
        public FactsInformation(Type factType, string filter, string[]fields, Func<string[],object> creator)
        {
            FactType = factType;
            Filter = filter;
            Fields = fields;
            Creator = creator;
        }

        public Type FactType { get; }
        public string Filter { get;}
        public string[] Fields { get; }
        public Func<string[], object> Creator { get; }
    }
}
