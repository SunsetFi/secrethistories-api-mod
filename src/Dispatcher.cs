namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// A dispatcher for running tasks on the main thread.
    /// </summary>
    // TODO: This is old code from stationeers webapi.  Autoccultist uses a better method using schedulers, switch to that.
    public class Dispatcher : MonoBehaviour
    {
#if CS
        private static Dispatcher instance;
#endif

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
            // This might be dangerous, but nothing bad seems to have happened yet.
            return Task.FromResult(function());
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
        public static Task DispatchWrite(Action function)
        {
            return RunOnMainThread(function);
        }

        /// <summary>
        /// Initializes the dispatcher.
        /// </summary>
        public static void Initialize()
        {
            // BH deletes game objects on load, so we dispatch from Bep instead.
#if CS
            if (instance == null)
            {

                instance = new GameObject("Dispatcher").AddComponent<Dispatcher>();
                DontDestroyOnLoad(instance.gameObject);
            }
#endif
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
        public static Task RunOnMainThread(Action action)
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

#if CS
        private void OnDestroy()
        {
            Logging.LogTrace($"Dispatcher destroyed by. {new StackTrace().ToString()}");
        }

        private void Update()
        {
            Drain();
        }
#endif

        private class QueuedTask
        {
            public Func<object> Function { get; set; }

            public TaskCompletionSource<object> CompletionSource { get; set; }
        }
    }
}
