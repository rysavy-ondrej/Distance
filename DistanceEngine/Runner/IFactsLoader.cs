using System;

namespace Distance.Engine.Runner
{
    public interface IFactsLoader
    {
        IObservable<object> GetData(string pcapPath);
    }
}