namespace SHRestAPI.Payloads
{
    using SecretHistories.Abstract;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server.Exceptions;
    using static SHRestAPI.SafeFucinePath;

    /// <summary>
    /// Translation strategy for the <see cref="ITokenPayload"/> interface.
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
        /// <param name="payload">The payload.</param>
        /// <returns>The full path to the sphere.</returns>
        [JsonPropertyGetter("spherePath")]
        public string GetPath(ITokenPayload payload)
        {
            return payload.GetToken().Sphere.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Gets a value indicating if this token is in an exterior sphere.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>A value indicating whether the token is in an exterior sphere.</returns>
        [JsonPropertyGetter("inExteriorSphere")]
        public bool GetInExteriorSphere(ITokenPayload payload)
        {
            return payload.GetToken().Sphere.IsExteriorSphere;
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

            var token = payload.GetToken();

            var currentSphere = token.Sphere;
            if (!currentSphere.IsExteriorSphere)
            {
                throw new ConflictException($"The ITokenPayload {token.PayloadTypeName} {token.PayloadEntityId} is not in an exterior sphere.");
            }

            token.RequestHomeLocationFromCurrentSphere();
            if (!fucinePath.TargetSphere.TryAcceptToken(token, new Context(Context.ActionSource.PlayerDrag)))
            {
                throw new BadRequestException($"The ITokenPayload {token.PayloadTypeName} {token.PayloadEntityId} could not be moved to sphere \"{path}\".");
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

        /// <summary>
        /// Gets the space occupation type of this token.
        /// </summary>
        /// <param name="payload">The token payload.</param>
        /// <returns>The type of space this token occupies.</returns>
        [JsonPropertyGetter("occupiesSpaceAs")]
        public string GetOccupiesSpaceAs(ITokenPayload payload)
        {
            return payload.GetToken().OccupiesSpaceAs().ToString();
        }
    }
}
