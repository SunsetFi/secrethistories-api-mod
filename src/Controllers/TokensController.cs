namespace SHRestAPI.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Endpoints for token fetching.
    /// </summary>
    [WebController(Path = "api/tokens")]
    public class TokensController
    {
        /// <summary>
        /// Gets all tokens according to filter query params.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request has been handled.</returns>
        [WebRouteMethod(Method = "GET")]
        public async Task GetAllTokens(IHttpContext context)
        {
            context.QueryString.TryGetValue("skip", out var skipStr);
            int.TryParse(skipStr, out var skip);
            context.QueryString.TryGetValue("limit", out var limitStr);
            int.TryParse(limitStr, out var limit);

            context.QueryString.TryGetValue("fucinePath", out var fucinePath);
            string[] fucinePaths = string.IsNullOrEmpty(fucinePath) ? new string[0] : fucinePath.Split(",");

            context.QueryString.TryGetValue("spherePrefix", out var spherePrefix);
            string[] spherePrefixes = string.IsNullOrEmpty(spherePrefix) ? new string[0] : spherePrefix.Split(",");

            context.QueryString.TryGetValue("payloadType", out var payloadType);
            string[] payloadTypes = string.IsNullOrEmpty(payloadType) ? new string[0] : payloadType.Split(",");

            context.QueryString.TryGetValue("elementId", out var elementId);
            string[] elementIds = string.IsNullOrEmpty(elementId) ? new string[0] : elementId.Split(",");

            context.QueryString.TryGetValue("verbId", out var verbId);
            string[] verbIds = string.IsNullOrEmpty(verbId) ? new string[0] : verbId.Split(",");

            var result = await Dispatcher.DispatchRead(() =>
            {
                // This may throw web errors if paths are invalid.
                var targetPaths = fucinePaths.Select(SafeFucinePath.WebSafeParse).ToArray();

                IEnumerable<Sphere> fromSpheres = FucineRoot.Get().GetSpheres();
                if (targetPaths.Length > 0)
                {
                    fromSpheres = from path in targetPaths
                                  from sphere in path.GetSpheresAtPath()
                                  select sphere;
                }

                // We want to consider tokens AT fucinePath to be part of the path.
                // GetSpheresAtPath returns the pheres of that token, not the token itself.
                // Note: If we have overlapping paths, we might get a token multiple times.
                // We should probably run a final Distinct() on the result...
                var targetTokens = fromSpheres.SelectMany(TokenUtils.GetAllTokens).Concat(targetPaths.Select(x => x.TargetToken).Where(x => x != null));

                IEnumerable<Token> query = from token in targetTokens
                                           where spherePrefixes.Length == 0 || spherePrefixes.Any(id => token.Sphere.GetAbsolutePath().Path.StartsWith(id))
                                           where payloadTypes.Length == 0 || payloadTypes.Any(type => token.PayloadTypeName == type)
                                           where elementIds.Length == 0 || (token.Payload is ElementStack elementStack && elementIds.Any(id => elementStack.Element.Id == id))
                                           where verbIds.Length == 0 || (token.Payload is Situation situation && verbIds.Any(id => situation.Verb.Id == id))
                                           select token;

                if (skip > 0)
                {
                    query = query.Skip(skip);
                }

                if (limit > 0)
                {
                    query = query.Take(limit);
                }

                return query.Select(TokenUtils.TokenToJObject).ToArray();
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets a token by its id.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="tokenId">The id of the token to get.</param>
        /// <returns>A task that resolves when the request has been handled.</returns>
        [WebRouteMethod(Method = "GET", Path = ":tokenId")]
        public async Task GetTokenById(IHttpContext context, string tokenId)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var token = TokenUtils.GetAllTokens().FirstOrDefault(t => t.PayloadId == tokenId);
                if (token == null)
                {
                    throw new NotFoundException($"Token {tokenId} not found.");
                }

                return TokenUtils.TokenToJObject(token);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Updates a token by its id.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="tokenId">The id of the token to update.</param>
        /// <returns>A task that resolves when the request has been handled.</returns>
        [WebRouteMethod(Method = "PATCH", Path = ":tokenId")]
        public async Task UpdateToken(IHttpContext context, string tokenId, JObject body)
        {
            var result = await Dispatcher.DispatchWrite(() =>
            {
                var token = TokenUtils.GetAllTokens().FirstOrDefault(t => t.PayloadId == tokenId);
                if (token == null)
                {
                    throw new NotFoundException($"Token {tokenId} not found.");
                }

                TokenUtils.UpdateToken(body, token);

                return TokenUtils.TokenToJObject(token);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
