namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Ceen;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// A controller dealing with situations.
    /// </summary>
    [WebController(Path = "api/situations")]
    public class SituationController
    {
        /// <summary>
        /// Gets all situations.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "GET")]
        public async Task GetSituations(IHttpContext context)
        {
            var result = Dispatcher.RunOnMainThread(() => (from situation in Watchman.Get<HornedAxe>().GetRegisteredSituations()
                                                           let json = JsonTranslator.ObjectToJson(situation)
                                                           select json).ToArray());
            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets a situation by ID.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="situationId">The situation ID.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = ":situationId")]
        public async Task GetSituationById(IHttpContext context, string situationId)
        {
            var result = Dispatcher.RunOnMainThread(() =>
            {
                var situation = Watchman.Get<HornedAxe>().GetRegisteredSituations().FirstOrDefault(x => x.VerbId == situationId);
                if (situation == null)
                {
                    throw new NotFoundException($"Situation {situationId} not found.");
                }

                return JsonTranslator.ObjectToJson(situation);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
