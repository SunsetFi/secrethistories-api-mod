namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Source of events for Cultist Simulator.
    /// </summary>
    public static class GameEventSource
    {
        /// <summary>
        /// Raised when the game proper starts.
        /// </summary>
        public static event EventHandler<EventArgs> GameStarted;

        /// <summary>
        /// Raised every game tick.
        /// </summary>
        public static event EventHandler<EventArgs> GameTick;

        /// <summary>
        /// Raised when the game proper ends.
        /// </summary>
        public static event EventHandler<EventArgs> GameEnded;

        private static List<Action> gameStartedSubscribers = new();

        /// <summary>
        /// Raise the <see cref="GameStarted"/> event.
        /// </summary>
        public static void RaiseGameStarted()
        {
            GameStarted?.Invoke(null, EventArgs.Empty);
            foreach (var subscriber in gameStartedSubscribers)
            {
                subscriber();
            }
            gameStartedSubscribers.Clear();
        }

        public static Task AwaitGameStarted()
        {
            var source = new TaskCompletionSource<bool>();
            gameStartedSubscribers.Add(() => source.SetResult(true));
            return source.Task;
        }

        /// <summary>
        /// Raise the <see cref="GameTick"/> event.
        /// </summary>
        public static void RaiseGameTick()
        {
            GameTick?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Raise the <see cref="GameEnded"/> event.
        /// </summary>
        public static void RaiseGameEnded()
        {
            GameEnded?.Invoke(null, EventArgs.Empty);
        }
    }
}
