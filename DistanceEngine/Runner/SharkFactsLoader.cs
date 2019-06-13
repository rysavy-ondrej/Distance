using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distance.Engine.Runner
{

    class SharkFactsLoader : IFactsLoader
    {
        public static readonly Char Separator = '\t';
        private readonly FactsInformation[] m_factInfoCollection;
        private readonly Action<FactsInformation> m_onStarted;
        private readonly Action<FactsInformation, int> m_onCompleted;

        public SharkFactsLoader(IEnumerable<FactsInformation> factInfoCollection, Action<FactsInformation> onStarted = null, Action<FactsInformation, int> onCompleted = null)
        {
            m_factInfoCollection = factInfoCollection.ToArray();
            m_onStarted = onStarted;
            m_onCompleted = onCompleted;
        }

        public IObservable<object> GetData(string pcapPath)
        {
            var providers = m_factInfoCollection.ToObservable();
            return providers.SelectMany(x => RunShark(pcapPath, x).Select(line => x.Creator(line.Split(Separator))));
        }

        private IObservable<string> RunShark(string inputfile, FactsInformation info)
        {
            return Observable.Create<string>(observer => Task.Run(async () => 
            {
                var fieldString = String.Join(" -e ", info.Fields);
                var arguments = $"-r {inputfile} -Y {info.Filter} -T fields -e {fieldString}";
                var count = 0;
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "tshark",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                m_onStarted?.Invoke(info);
                while (true)
                {
                    string line = await process.StandardOutput.ReadLineAsync();
                    if (line != null)
                    {
                        count++;
                        observer.OnNext(line);
                    }
                    else
                    {
                        m_onCompleted?.Invoke(info, count);
                        observer.OnCompleted();
                        break;
                    }
                }
                await process.WaitForExitAsync();
                return Disposable.Create(() => { });
            }));
        }

    }

    static class Extensions
    {
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default)
                cancellationToken.Register(tcs.SetCanceled);

            return tcs.Task;
        }
    }
}
