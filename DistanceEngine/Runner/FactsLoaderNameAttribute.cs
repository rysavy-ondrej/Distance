using System;

namespace Distance.Engine.Runner
{
    internal class FactsLoaderAttribute : Attribute
    {
        private string m_loaderName;

        public FactsLoaderAttribute(string loaderName)
        {
            m_loaderName = loaderName;
        }
    }
}
