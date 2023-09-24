namespace SHRestAPI
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using SHRestAPI.Tasks;

    /// <summary>
    /// Utilities for long polling game data.
    /// </summary>
    public static class Polling
    {
        /// <summary>
        /// Polls a value until it changes.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="poll">The function to fetch the data on the main thread.</param>
        /// <param name="previousHash">The previous hash value.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="resolution">How often to poll.</param>
        /// <param name="computeHash">A function for computing the hash.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<Tuple<int, T>> Poll<T>(Func<T> poll, int previousHash, int timeout, int resolution = 1000, Func<T, int> computeHash = null)
        {
            if (timeout <= 0)
            {
                timeout = 10000;
            }

            if (resolution <= 0)
            {
                resolution = 1000;
            }

            var cts = new CancellationTokenSource();

            var task = PollValue(previousHash, resolution, poll, computeHash, cts.Token);
            var timeoutTask = RealtimeDelay.Of(timeout, cts.Token);

            var completedTask = await Task.WhenAny(task, timeoutTask);

            // Cancel whatever task didn't complete.
            cts.Cancel();

            if (completedTask == task)
            {
                return await task;
            }
            else
            {
                return await GetValue(poll, computeHash);
            }
        }

        private static async Task<Tuple<int, T>> PollValue<T>(int hash, int resolution, Func<T> poll, Func<T, int> computeHash, CancellationToken cancellationToken)
        {
            while (true)
            {
                await RealtimeDelay.Of(TimeSpan.FromMilliseconds(resolution), cancellationToken);
                var tuple = await GetValue(poll, computeHash);
                if (tuple.Item1 != hash)
                {
                    return tuple;
                }
            }
        }

        private static async Task<Tuple<int, T>> GetValue<T>(Func<T> poll, Func<T, int> computeHash)
        {
            var value = await Dispatcher.DispatchRead(poll);
            var valueHash = computeHash != null ? computeHash(value) : value.GetHashCode();
            return new Tuple<int, T>(valueHash, value);
        }
    }
}
