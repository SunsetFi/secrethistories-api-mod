namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Services;
    using SecretHistories.UI;
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
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                string legacyId = null;
                string legacyLabel = null;
                var stageHand = Watchman.Get<StageHand>();
                if (stageHand.SceneIsActive(Constants.GameScene))
                {
                    var protag = Watchman.Get<Stable>().Protag();
                    legacyId = protag.ActiveLegacy.Id;
                    legacyLabel = protag.ActiveLegacy.Label;
                }

                return new
                {
                    legacyId,
                    legacyLabel,
                };
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets all elements manifested this game.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements-manifested")]
        public async Task GetUniqueElementsManifested(IHttpContext context)
        {
            var result = await Dispatcher.RunOnMainThread(() =>
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
            var result = await Dispatcher.RunOnMainThread(() =>
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
            var result = await Dispatcher.RunOnMainThread(() =>
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
