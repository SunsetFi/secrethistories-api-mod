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
        [WebRouteMethod(Method = "GET", Path = "manifested-elements")]
        public async Task GetUniqueElementsManifested(IHttpContext context)
        {
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var character = Watchman.Get<Stable>().Protag();
                return character.UniqueElementsManifested.ToArray();
            });

            await context.SendResponse(HttpStatusCode.OK, new
            {
                result,
            });
        }

        /// <summary>
        /// Gets all recipes executed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "executed-recipes")]
        public async Task GetRecipesExecuted(IHttpContext context)
        {
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var character = Watchman.Get<Stable>().Protag();
                return from pair in character.RecipeExecutions
                       select new
                       {
                           recipe = pair.Key,
                           count = pair.Value,
                       };
            });

            await context.SendResponse(HttpStatusCode.OK, new
            {
                result,
            });
        }
    }
}
