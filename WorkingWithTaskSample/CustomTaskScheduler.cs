using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WorkingWithTaskSample
{
	public class CustomTaskScheduler : TaskScheduler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        protected override async void QueueTask(Task task)
        {
            await _semaphore.WaitAsync();
            try
            {
                await Task.Run(() => base.TryExecuteTask(task));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;

        protected override IEnumerable<Task> GetScheduledTasks() { yield break; }
    }
}
