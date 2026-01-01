using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace StrangeSpace
{


    public class TaskRunner
    {
        private CancellationTokenSource _cts;

        public bool IsRunning => _cts != null;

        public void TryLaunch(System.Func<CancellationToken, UniTask> taskFactory)
        {
            if (_cts != null) return;

            _cts = new CancellationTokenSource();
            RunAsync(taskFactory, _cts.Token).Forget();
        }

        private async UniTaskVoid RunAsync(System.Func<CancellationToken, UniTask> taskFactory, CancellationToken token)
        {
            try
            {
                await taskFactory(token);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
        }
    }
}