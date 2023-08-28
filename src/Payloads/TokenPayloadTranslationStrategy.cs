namespace SHRestAPI.Payloads
{
    using SecretHistories.Abstract;
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server.Exceptions;
    using static SHRestAPI.SafeFucinePath;

    /// <summary>
    /// Translation strategy for the <see cref="payload"/> class.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(ITokenPayload))]
    public class TokenPayloadTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the payload.GetToken().
        /// </summary>
        /// <param name="payload">The payload.GetToken().</param>
        /// <returns>The ID of the payload.GetToken().</returns>
        [JsonPropertyGetter("id")]
        public string GetId(ITokenPayload payload)
        {
            return payload.Id;
        }

        /// <summary>
        /// Gets the fucine path of the payload.GetToken().
        /// </summary>
        /// <param name="payload">The payload.GetToken().</param>
        /// <returns>The full path to the payload.GetToken().</returns>
        [JsonPropertyGetter("path")]
        public string GetTokenPath(ITokenPayload payload)
        {
            return payload.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Gets the fucine path of the containing sphere.
        /// </summary>
        /// <param name="payload">The payload.GetToken().</param>
        /// <returns>The full path to the sphere.</returns>
        [JsonPropertyGetter("spherePath")]
        public string GetPath(ITokenPayload payload)
        {
            return payload.GetToken().Sphere.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Sets the path of the payload.GetToken().
        /// This effectively moves the ITokenPayload to the sphere.
        /// This may have side effects, and the sphere might reject the payload.GetToken().
        /// </summary>
        /// <param name="payload">The payload.GetToken().</param>
        /// <param name="path">The path to move the ITokenPayload to.</param>
        [JsonPropertySetter("spherePath")]
        public void SetPath(ITokenPayload payload, string path)
        {
            SafeFucinePath fucinePath;
            try
            {
                fucinePath = new SafeFucinePath(path);
            }
            catch (PathElementNotFoundException)
            {
                throw new BadRequestException($"No sphere found at path \"{path}\".");
            }

            if (fucinePath.TargetSphere == null)
            {
                throw new BadRequestException($"No sphere found at path \"{path}\".");
            }

            if (!fucinePath.TargetSphere.TryAcceptToken(payload.GetToken(), new Context(Context.ActionSource.PlayerDrag)))
            {
                throw new BadRequestException($"The ITokenPayload could not be moved to sphere \"{path}\".");
            }
        }

        /// <summary>
        /// Gets the type of the token's payload.GetToken().
        /// </summary>
        /// <param name="payload">The ITokenPayload to get the payload type of.</param>
        /// <returns>The token's payload type.</returns>
        [JsonPropertyGetter("payloadType")]
        public string GetPayloadType(ITokenPayload payload)
        {
            return payload.GetType().Name;
        }
    }
}
