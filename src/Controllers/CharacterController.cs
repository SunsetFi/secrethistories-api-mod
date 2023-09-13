namespace SHRestAPI.Controllers
{
    using System.Collections.Generic;
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
        /// Gets all recipes executed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "recipes-executed")]
        public async Task GetRecipesExecuted(IHttpContext context)
        {
#if BH
            // FIXME: This was just removed.  In theory we might get some sort of replacement, as the
            // game is getting a recipe book of sorts.
            var result = new Dictionary<string, int>();
#else
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var character = Watchman.Get<Stable>().Protag();

                // Clone the dict so we dont access data from the wrong thread.
                return character.RecipeExecutions.ToDictionary(x => x.Key, x => x.Value);
            });
#endif

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
