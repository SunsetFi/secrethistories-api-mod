namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Ceen;
    using SecretHistories.UI;
    using SHRestAPI.Server.Attributes;

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
            context.Request.QueryString.TryGetValue("spherePrefix", out var spherePrefix);
            string[] spheres = string.IsNullOrEmpty(spherePrefix) ? new string[0] : spherePrefix.Split(",");

            context.Request.QueryString.TryGetValue("payloadType", out var payloadType);
            string[] payloadTypes = string.IsNullOrEmpty(payloadType) ? new string[0] : payloadType.Split(",");

            context.Request.QueryString.TryGetValue("elementId", out var elementId);
            string[] elementIds = string.IsNullOrEmpty(elementId) ? new string[0] : elementId.Split(",");

            var result = Dispatcher.RunOnMainThread(() =>
            {
                return (from token in TokenUtils.GetAllTokens()
                        where spheres.Length == 0 || spheres.Any(id => token.Sphere.GetAbsolutePath().Path.StartsWith(id))
                        where payloadTypes.Length == 0 || payloadTypes.Any(type => token.PayloadTypeName == type)
                        where elementIds.Length == 0 || (token.Payload is ElementStack elementStack && elementIds.Any(id => elementStack.Element.Id == id))
                        select TokenUtils.TokenToJObject(token)).ToArray();
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
