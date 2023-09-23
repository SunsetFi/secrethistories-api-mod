namespace SHRestAPI
{
    using System;

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

        /// <summary>
        /// Raise the <see cref="GameStarted"/> event.
        /// </summary>
        public static void RaiseGameStarted()
        {
            GameStarted?.Invoke(null, EventArgs.Empty);
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
