namespace SHRestAPI.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a polling task that resolves once a frame has passed.
    /// </summary>
    public class AwaitFrameTask : GlobalUpdateTask<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AwaitFrameTask"/> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected AwaitFrameTask(CancellationToken cancellationToken)
            : base(cancellationToken)
        {
            GameEventSource.GameEnded += this.HandleGameEnded;
        }

        /// <summary>
        /// Creates a new task that will resolve once a frame has passed.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task.</returns>
        public static Task From(CancellationToken cancellationToken)
        {
            return new AwaitFrameTask(cancellationToken).Task;
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
            this.SetResult(true);
        }

        /// <inheritdoc/>
        protected override void OnDisposed()
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
        }

        private void HandleGameEnded(object sender, EventArgs e)
        {
            GameEventSource.GameEnded -= this.HandleGameEnded;
            this.SetException(new Exception("Game ended while awaiting AwaitFrameTask."));
        }
    }
}
