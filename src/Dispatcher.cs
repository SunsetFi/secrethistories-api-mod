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
        private static Dispatcher instance;
        private static volatile bool queued = false;
        private static List<QueuedTask> backlog = new List<QueuedTask>(8);
        private static List<QueuedTask> actions = new List<QueuedTask>(8);

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
        public static Task<T> RunOnMainThread<T>(Func<T> function)
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
            return source.Task.ContinueWith(t => (T)t.Result);
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

        private void OnDestroy()
        {
            Logging.LogTrace($"Dispatcher destroyed by. {new StackTrace().ToString()}");
        }

        private void Update()
        {
            Drain();
        }

        private class QueuedTask
        {
            public Func<object> Function { get; set; }

            public TaskCompletionSource<object> CompletionSource { get; set; }
        }
    }
}
