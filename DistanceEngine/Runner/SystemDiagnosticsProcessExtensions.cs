using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Distance.Engine.Runner
{
    static class SystemDiagnosticsProcessExtensions
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
