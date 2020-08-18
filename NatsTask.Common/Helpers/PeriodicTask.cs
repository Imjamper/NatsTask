using System;
using System.Threading;
using System.Threading.Tasks;

namespace NatsTask.Common.Helpers
{
    public class PeriodicTask
    {
        public static Task StartPeriodicTask(Action action, int intervalInMilliseconds, int delayInMilliseconds,
            CancellationToken cancelToken)
        {
            void WrapperAction()
            {
                if (cancelToken.IsCancellationRequested) return;

                action();
            }

            void MainAction()
            {
                var attachedToParent = TaskCreationOptions.AttachedToParent;

                if (cancelToken.IsCancellationRequested) return;

                if (delayInMilliseconds > 0) Thread.Sleep(delayInMilliseconds);

                while (true)
                {
                    if (cancelToken.IsCancellationRequested) break;

                    Task.Factory.StartNew(WrapperAction, cancelToken, attachedToParent, TaskScheduler.Current);

                    if (cancelToken.IsCancellationRequested || intervalInMilliseconds == Timeout.Infinite) break;

                    Thread.Sleep(intervalInMilliseconds);
                }
            }

            return Task.Factory.StartNew(MainAction, cancelToken);
        }
    }
}