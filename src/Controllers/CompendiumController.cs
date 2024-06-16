namespace SHRestAPI.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;
    using UnityEngine;

    /// <summary>
    /// A controller dealing with information from the compendium.
    /// </summary>
    [WebController(Path = "api/compendium")]
    public class CompendiumController
    {
        private Compendium Compendium => Watchman.Get<Compendium>();

        /// <summary>
        /// Gets all elements in the compendium.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements")]
        public async Task GetElements(IHttpContext context)
        {
            bool? isAspect = context.QueryString.ContainsKey("isAspect") ? bool.Parse(context.QueryString["isAspect"]) : null;
            bool? isHidden = context.QueryString.ContainsKey("isHidden") ? bool.Parse(context.QueryString["isHidden"]) : null;
            var result = await Dispatcher.DispatchRead(() =>
            {
                var elements = this.Compendium.GetEntitiesAsList<Element>();
                return from element in elements
                       where !isAspect.HasValue || element.IsAspect == isAspect
                       where !isHidden.HasValue || element.IsHidden == isHidden
                       select JsonTranslator.ObjectToJson(element);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets an element by its id.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="elementId">The id of the element to get.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements/:elementId")]
        public async Task GetElement(IHttpContext context, string elementId)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var element = this.Compendium.GetEntityById<Element>(elementId);
                if (element == null || !element.IsValid())
                {
                    throw new NotFoundException($"Element with id {elementId} not found.");
                }

                return JsonTranslator.ObjectToJson(element);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets the icon of an element.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="elementId">The id of the element to get the icon for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements/:elementId/icon.png")]
        public async Task GetElementIcon(IHttpContext context, string elementId)
        {
            var result = await Dispatcher.DispatchGraphicsRead(() =>
            {
                var element = this.Compendium.GetEntityById<Element>(elementId);
                if (element == null || !element.IsValid())
                {
                    throw new NotFoundException($"Element with id {elementId} not found.");
                }

                var sprite = ResourcesManager.GetAppropriateSpriteForElement(element);
                return sprite.ToTexture().EncodeToPNG();
            });

            await context.SendResponse(HttpStatusCode.OK, "image/png", new MemoryStream(result));
        }

        /// <summary>
        /// Gets all verbs in the compendium.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "verbs")]
        public async Task GetVerbs(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var verbs = this.Compendium.GetEntitiesAsList<Verb>();
                return from verb in verbs
                       select JsonTranslator.ObjectToJson(verb);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets a verb by its id.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="verbId">The id of the verb to get.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "verbs/:verbId")]
        public async Task GetVerb(IHttpContext context, string verbId)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var verb = this.Compendium.GetEntityById<Verb>(verbId);
                if (verb == null || !verb.IsValid())
                {
                    throw new NotFoundException($"Verb with id {verbId} not found.");
                }

                return JsonTranslator.ObjectToJson(verb);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets the icon for a verb.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="verbId">The id of the verb to get the icon for.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "verbs/:verbId/icon.png")]
        public async Task GetVerbIcon(IHttpContext context, string verbId)
        {
            var result = await Dispatcher.DispatchGraphicsRead(() =>
            {
                var verb = this.Compendium.GetEntityById<Verb>(verbId);
                if (verb == null || !verb.IsValid())
                {
                    throw new NotFoundException($"Verb with id {verbId} not found.");
                }

                var sprite = ResourcesManager.GetSpriteForVerbLarge(verb.Icon ?? verb.Id);
                return sprite.ToTexture().EncodeToPNG();
            });

            await context.SendResponse(HttpStatusCode.OK, "image/png", new MemoryStream(result));
        }

        /// <summary>
        /// Gets all recipes in the compendium.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "recipes")]
        public async Task GetRecipes(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var recipes = this.Compendium.GetEntitiesAsList<Recipe>();
                return from recipe in recipes
                       select JsonTranslator.ObjectToJson(recipe);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets a recipe by id.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="recipeId">The ID of the recipe to get.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "recipes/:recipeId")]
        public async Task GetRecipe(IHttpContext context, string recipeId)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var recipe = this.Compendium.GetEntityById<Recipe>(recipeId);
                if (recipe == null || !recipe.IsValid())
                {
                    throw new NotFoundException($"Recipe with id {recipeId} not found.");
                }

                return JsonTranslator.ObjectToJson(recipe);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }
    }
}
