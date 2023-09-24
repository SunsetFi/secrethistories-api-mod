namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;

    /// <summary>
    /// Controller for fetching character data.
    /// </summary>
    [WebController(Path = "api/character")]
    public class CharacterController
    {
        /// <summary>
        /// Gets the active legacy, if any.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "GET", Path = "legacy")]
        public async Task GetLegacy(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
                        {
                            var stageHand = Watchman.Get<StageHand>();
                            if (stageHand.SceneIsActive(Constants.GameScene))
                            {
                                var protag = Watchman.Get<Stable>().Protag();
                                return new
                                {
                                    legacyId = protag.ActiveLegacy.Id,
                                    legacyLabel = protag.ActiveLegacy.Label,
                                };
                            }

                            return null;
                        });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Polls the legacy by delaying the response until either the legacy changes or the timeout is reached.
        /// </summary>
        /// <param name="context">The HTTP Context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "GET", Path = "legacy/poll")]
        public async Task PollLegacy(IHttpContext context)
        {
            context.QueryString.TryGetValue("timeout", out var timeoutStr);
            int.TryParse(timeoutStr, out var timeout);

            context.QueryString.TryGetValue("resolution", out var resolutionStr);
            int.TryParse(resolutionStr, out var resolution);

            context.QueryString.TryGetValue("previousHash", out var previousHashStr);
            int.TryParse(previousHashStr, out var previousHash);

            var result = await Polling.Poll(
                () =>
                {
                    var stageHand = Watchman.Get<StageHand>();
                    if (stageHand.SceneIsActive(Constants.GameScene))
                    {
                        var protag = Watchman.Get<Stable>().Protag();
                        return new
                        {
                            legacyId = protag.ActiveLegacy.Id,
                            legacyLabel = protag.ActiveLegacy.Label,
                        };
                    }

                    return null;
                },
                previousHash,
                timeout,
                resolution,
                x => x != null ? x.legacyId.GetHashCode() : 0);

            await context.SendResponse(HttpStatusCode.OK, new
            {
                hash = result.Item1,
                legacy = result.Item2,
            });
        }

        /// <summary>
        /// Gets all elements manifested this game.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements-manifested")]
        public async Task GetUniqueElementsManifested(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var character = Watchman.Get<Stable>().Protag();
                return character.UniqueElementsManifested.ToArray();
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

#if BH
        /// <summary>
        /// Gets all ambittable recipes unlocked.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "ambittable-recipes-unlocked")]
        public async Task GetAmbittableRecipesUnlocked(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var character = Watchman.Get<Stable>().Protag();
                return character.AmbittableRecipesUnlocked.ToArray();
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
#endif

        /// <summary>
        /// Gets all recipes executed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "recipes-executed")]
        public async Task GetRecipesExecuted(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var character = Watchman.Get<Stable>().Protag();
#if BH
                // Shim in the new way into the old way.
                return character.AmbittableRecipesUnlocked.ToDictionary(x => x, x => 1);
#else
                // Clone the dict so we dont access data from the wrong thread.
                return character.RecipeExecutions.ToDictionary(x => x.Key, x => x.Value);
#endif
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
