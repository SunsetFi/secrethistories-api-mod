namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// A dispatcher for running tasks on the main thread.
    /// </summary>
    // TODO: This is old code from stationeers webapi.  Autoccultist uses a better method using schedulers, switch to that.
    public static class Dispatcher
    {
        private static volatile bool queued = false;
        private static List<QueuedTask> backlog = new List<QueuedTask>(8);
        private static List<QueuedTask> actions = new List<QueuedTask>(8);

        /// <summary>
        /// Dispatches a read operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="function">The function to dispatch.</param>
        /// <returns>The return value.</returns>
        public static Task<T> DispatchRead<T>(Func<T> function)
        {
            // We somehow managed to corrupt the game state doing reads, so totally giving up on threaded reading
            // This was working fine up until we added canExecute to Situation translation, which touches and updates cached content.
            // return RunOnMainThread(function);

            // This is risky.  We have found a few places where global caches of objects are being used, causing reads
            // to cause list iterator invalidations.
            try
            {
                return Task.FromResult(function());
            }
            catch (Exception ex)
            {
                return Task.FromException<T>(ex);
            }
        }

        /// <summary>
        /// Dispatches a read operation that uses graphics.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="function">The function to dispatch.</param>
        /// <returns>The return value.</returns>
        public static Task<T> DispatchGraphicsRead<T>(Func<T> function)
        {
            return RunOnMainThread(function);
        }

        /// <summary>
        /// Dispatches a write operation.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="function">The function to dispatch.</param>
        /// <returns>The return value.</returns>
        public static Task<T> DispatchWrite<T>(Func<T> function)
        {
            return RunOnMainThread(function);
        }

        /// <summary>
        /// Dispatches a write operation.
        /// </summary>
        /// <param name="function">The function to dispatch.</param>
        /// <returns>The task.</returns>
        public static Task<object> DispatchWrite(Action function)
        {
            return RunOnMainThread(function);
        }

        /// <summary>
        /// Executes all pending items in the dispatch.
        /// </summary>
        public static void Drain()
        {
            if (queued)
            {
                lock (backlog)
                {
                    var tmp = actions;
                    actions = backlog;
                    backlog = tmp;
                    queued = false;
                }

                foreach (var action in actions)
                {
                    try
                    {
                        var result = action.Function();
                        action.CompletionSource.TrySetResult(result);
                    }
                    catch (Exception e)
                    {
                        action.CompletionSource.TrySetException(e);
                    }
                }

                actions.Clear();
            }
        }

        /// <summary>
        /// Run the action on the main thread.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <returns>A task that completes when the action has finished.</returns>
        public static Task<object> RunOnMainThread(Action action)
        {
            return RunOnMainThread<object>(() =>
            {
                action();
                return null;
            });
        }

        /// <summary>
        /// Run the function on the main thread.
        /// </summary>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <param name="function">The function to run.</param>
        /// <returns>A task that resolves to the return value of the function.</returns>
        private static Task<T> RunOnMainThread<T>(Func<T> function)
        {
            var source = new TaskCompletionSource<object>();
            var queueItem = new QueuedTask()
            {
                Function = () => function(),
                CompletionSource = source,
            };

            lock (backlog)
            {
                backlog.Add(queueItem);
                queued = true;
            }

            // Sigh...
            // FIXME: I think our tasks are continuing on the main thread.
            // Might need a ConfigureAwait here.
            return source.Task.ContinueWith(t => (T)t.Result);
        }

        private class QueuedTask
        {
            public Func<object> Function { get; set; }

            public TaskCompletionSource<object> CompletionSource { get; set; }
        }
    }
}
