using System;
namespace Distance.Engine
{
    /// <summary>
    /// Creates fact loaders for the specified fact definitions.
    /// </summary>
    public class FactLoaderFactory
    {
        public FactLoaderFactory()
        {
        }

        IFactLoader CreateFactLoader(string yaml)
        {
            throw new NotImplementedException();
        }
    }
}
