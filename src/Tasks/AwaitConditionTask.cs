namespace SHRestAPI.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a polling task that resolves once a condition is met.
    /// </summary>
    public class AwaitConditionTask : GlobalUpdateTask<bool>
    {
        private readonly Func<bool> condition;

        /// <summary>
        /// Initializes a new instance of the <see cref="AwaitConditionTask"/> class.
        /// </summary>
        /// <param name="condition">The condition to poll for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected AwaitConditionTask(Func<bool> condition, CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            this.condition = condition;
            GameEventSource.GameEnded += this.HandleGameEnded;
        }

        /// <summary>
        /// Creates a new task that will resolve once the condition returns true.
        /// </summary>
        /// <param name="condition">The condition to check for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public static Task From(Func<bool> condition, CancellationToken cancellationToken)
        {
            return new AwaitConditionTask(condition, cancellationToken).Task;
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            if (this.condition())
            {
                GameEventSource.GameEnded -= this.HandleGameEnded;
                this.SetResult(true);
            }
        }

        /// <inheritdoc/>
        protected override void OnDisposed()
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
        }

        private void HandleGameEnded(object sender, EventArgs e)
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
            this.SetException(new Exception("Game ended while awaiting AwaitConditionTask."));
        }
    }
}
