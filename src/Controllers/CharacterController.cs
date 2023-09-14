namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Ceen;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using SHRestAPI.Server.Attributes;

    /// <summary>
    /// Controller for fetching character data.
    /// </summary>
    [WebController(Path = "api/character")]
    public class CharacterController
    {
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
