namespace SHRestAPI.Payloads
{
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Translation strategy for the <see cref="Token"/> class.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Token))]
    public class TokenTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The ID of the token.</returns>
        [JsonPropertyGetter("id")]
        public string GetId(Token token)
        {
            return token.PayloadId;
        }

        /// <summary>
        /// Gets the fucine path of the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The full path to the token.</returns>
        [JsonPropertyGetter("path")]
        public string GetTokenPath(Token token)
        {
            return token.Payload.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Gets the fucine path of the containing sphere.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns>The full path to the sphere.</returns>
        [JsonPropertyGetter("spherePath")]
        public string GetPath(Token token)
        {
            return token.Sphere.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Sets the path of the token.
        /// This effectively moves the token to the sphere.
        /// This may have side effects, and the sphere might reject the token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="path">The path to move the token to.</param>
        [JsonPropertySetter("spherePath")]
        public void SetPath(Token token, string path)
        {
            var sphere = Watchman.Get<HornedAxe>().GetSphereByReallyAbsolutePathOrNullSphere(new FucinePath(path));
            if (sphere == null || !sphere.IsValid())
            {
                throw new BadRequestException($"No sphere found at path \"{path}\".");
            }

            if (!sphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
            {
                throw new BadRequestException($"The token could not be moved to sphere \"{path}\".");
            }
        }

        /// <summary>
        /// Gets the type of the token's payload.
        /// </summary>
        /// <param name="token">The token to get the payload type of.</param>
        /// <returns>The token's payload type.</returns>
        [JsonPropertyGetter("payloadType")]
        public string GetPayloadType(Token token)
        {
            return token.PayloadTypeName;
        }
    }
}
