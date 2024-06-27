namespace SHRestAPI.Tasks
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements a polling task that runs on the global update loop.
    /// </summary>
    /// <typeparam name="T">The task return type.</typeparam>
    public abstract class GlobalUpdateTask<T> : IDisposable
    {
        private readonly TaskCompletionSource<T> taskCompletionSource = new();
        private readonly CancellationToken cancellationToken;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalUpdateTask{T}"/> class.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected GlobalUpdateTask(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            GameEventSource.GameTick += this.OnUpdate;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GlobalUpdateTask{T}"/> class.
        /// </summary>
        ~GlobalUpdateTask()
        {
            this.Dispose();
        }

        /// <summary>
        /// Gets the task.
        /// </summary>
        public Task<T> Task => this.taskCompletionSource.Task;

        /// <summary>
        /// Gets a value indicating whether the task has been disposed.
        /// </summary>
        protected bool IsDisposed => this.isDisposed;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            GC.SuppressFinalize(this);

            this.taskCompletionSource.TrySetCanceled();

            GameEventSource.GameTick -= this.OnUpdate;

            this.OnDisposed();
        }

        /// <summary>
        /// Called when the task is disposed.
        /// </summary>
        protected virtual void OnDisposed()
        {
        }

        /// <summary>
        /// Called on every global update.
        /// </summary>
        protected abstract void Update();

        /// <summary>
        /// Sets the result of the task.
        /// </summary>
        /// <param name="value">The task result.</param>
        protected void SetResult(T value)
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetResult(value);
            this.Dispose();
        }

        /// <summary>
        /// Sets the task as canceled.
        /// </summary>
        protected void SetCanceled()
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetCanceled();
            this.Dispose();
        }

        /// <summary>
        /// Sets the task as faulted.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void SetException(Exception exception)
        {
            this.EnsureNotDisposed();

            this.taskCompletionSource.TrySetException(exception);
            this.Dispose();
        }

        /// <summary>
        /// Ensures that the task has not been disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(GlobalUpdateTask<T>));
            }
        }

        private void OnUpdate(object sender, EventArgs e)
        {
            if (this.cancellationToken.IsCancellationRequested)
            {
                this.SetCanceled();
                return;
            }

            try
            {
                this.Update();
            }
            catch (Exception ex)
            {
                this.SetException(ex);
            }
        }
    }
}
