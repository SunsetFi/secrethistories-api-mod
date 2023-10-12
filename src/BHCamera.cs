using System;

namespace SHRestAPI
{
    using System.Threading.Tasks;
    using SecretHistories.UI;
    using UnityEngine;

#if BH
    /// <summary>
    /// A class for controlling the camera in Book of Hours.
    /// </summary>
    public static class BHCamera
    {
        /// <summary>
        /// Focus the camera on the specified token.
        /// </summary>
        /// <param name="token">The token to focus on.</param>
        /// <returns>A task that resolves when the focus is completed.</returns>
        public static async Task FocusToken(Token token)
        {
            await PanToPosition(token.transform.position, 0.2f);
            await Zoom(Watchman.Get<CamOperator>().ZOOM_Z_QUITE_CLOSE, 0.1f);
        }

        private static Task PanToPosition(Vector2 position, float duration)
        {
            var camOperator = Watchman.Get<CamOperator>();
            var taskSource = new TaskCompletionSource<bool>();
            camOperator.PointCameraAt(position, duration, () => taskSource.SetResult(true));
            return taskSource.Task;
        }

        private static Task Zoom(float level, float duration)
        {
            var camOperator = Watchman.Get<CamOperator>();
            var taskSource = new TaskCompletionSource<bool>();
            camOperator.RequestZoom(level, camOperator.GetAttachedCamera().transform.position, duration, new Func<float, float>(Easing.Exponential.Out), () => taskSource.SetResult(true));
            return taskSource.Task;
        }
    }
#endif
}
