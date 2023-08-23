namespace SHRestAPI
{
    using UnityEngine;

    /// <summary>
    /// Extensions for the <see cref="Texture2D"/> class.
    /// </summary>
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Gets a readable texture from the given texture.
        /// </summary>
        /// <param name="texture">The texture to convert to readable.</param>
        /// <returns>A readable texture from the original.</returns>
        public static Texture2D ToReadable(this Texture2D texture)
        {
            if (texture.isReadable)
            {
                return texture;
            }

            // Create a temporary RenderTexture of the same size as the texture
            var tmp = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, tmp);

            // Backup the currently set RenderTexture
            var previous = RenderTexture.active;
            try
            {
                // Set the current RenderTexture to the temporary one we created
                RenderTexture.active = tmp;

                // Create a new readable Texture2D to copy the pixels to it
                var readable = new Texture2D(texture.width, texture.height);

                // Copy the pixels from the RenderTexture to the new Texture
                readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                readable.Apply();

                return readable;
            }
            finally
            {
                // Reset the active RenderTexture
                RenderTexture.active = previous;

                // Release the temporary RenderTexture
                RenderTexture.ReleaseTemporary(tmp);
            }
        }
    }
}
