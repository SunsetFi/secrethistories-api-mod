namespace SHRestAPI
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Utilities for working with tokens.
    /// </summary>
    public static class TokenUtils
    {
        /// <summary>
        /// Enumerates all tokens in the game.
        /// </summary>
        /// <returns>An enumerable of all tokens in the game.</returns>
        /// <remarks>
        /// This includes non-user spheres like the dealer's table.  Use with care.
        /// </remarks>
        public static IEnumerable<Token> GetAllTokens()
        {
            IEnumerable<Token> GetTokensDeep(Sphere sphere)
            {
                foreach (var token in sphere.Tokens)
                {
                    yield return token;

                    foreach (var childSphere in token.Payload.GetSpheres())
                    {
                        foreach (var childToken in GetTokensDeep(childSphere))
                        {
                            yield return childToken;
                        }
                    }
                }
            }

            foreach (var sphere in FucineRoot.Get().GetSpheres())
            {
                foreach (var token in GetTokensDeep(sphere))
                {
                    yield return token;
                }
            }
        }

        /// <summary>
        /// Create a token in a sphere from a payload.
        /// </summary>
        /// <param name="sphere">The target sphere.</param>
        /// <param name="payload">The payload json.</param>
        /// <returns>The created token serialized to json.</returns>
        public static JToken CreateSphereToken(Sphere sphere, JObject payload)
        {
            if (!payload.TryGetValue("payloadType", out var payloadType))
            {
                throw new BadRequestException("payloadType is required");
            }

            switch (payloadType.Value<string>())
            {
                case "ElementStack":
                    {
                        var item = payload.ToObject<ElementStackCreationPayload>();
                        item.Validate();
                        var token = item.Create(sphere);
                        return TokenToJObject(token);
                    }

                case "Situation":
                    {
                        var item = payload.ToObject<SituationCreationPayload>();
                        item.Validate();
                        var token = item.Create(sphere);
                        return TokenToJObject(token);
                    }
            }

            throw new BadRequestException($"Non-creatable payload type {payloadType}");
        }

        /// <summary>
        /// Update a token from a payload.
        /// </summary>
        /// <param name="body">The json to update from.</param>
        /// <param name="token">The token to update.</param>
        public static void UpdateToken(JObject body, Token token)
        {
            JsonTranslator.UpdateObjectFromJson(body, token.Payload, false);
        }

        /// <summary>
        /// Serialize a token as json.
        /// </summary>
        /// <param name="token">The token to serialize.</param>
        /// <returns>The serialized token.</returns>
        public static JObject TokenToJObject(Token token)
        {
            var json = new JObject();

            JsonTranslator.ObjectToJson(token.Payload, json);

            return json;
        }
    }
}
