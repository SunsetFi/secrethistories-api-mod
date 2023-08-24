namespace SHRestAPI
{
    using UnityEngine;

    /// <summary>
    /// Extensions for the <see cref="Sprite"/> class.
    /// </summary>
    public static class SpriteExtensions
    {
        /// <summary>
        /// Converts a sprite to a texture.
        /// </summary>
        /// <param name="sprite">The sprite to convert.</param>
        /// <returns>The texture from the sprite.</returns>
        public static Texture2D ToTexture(this Sprite sprite)
        {
            var rect = sprite.rect;
            var spriteTexture = sprite.texture.ToReadable();
            var newTexture = new Texture2D((int)rect.width, (int)rect.height);
            newTexture.SetPixels(spriteTexture.GetPixels((int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height));
            newTexture.Apply();
            return newTexture;
        }
    }
}
