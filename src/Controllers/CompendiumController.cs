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
            bool? isAspect = context.Request.QueryString.ContainsKey("isAspect") ? bool.Parse(context.Request.QueryString["isAspect"]) : null;
            bool? isHidden = context.Request.QueryString.ContainsKey("isHidden") ? bool.Parse(context.Request.QueryString["isHidden"]) : null;
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var elements = this.Compendium.GetEntitiesAsList<Element>();
                return from element in elements
                       where isAspect.HasValue && element.IsAspect == isAspect
                       where isHidden.HasValue && element.IsHidden == isHidden
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
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var element = this.Compendium.GetEntityById<Element>(elementId);
                if (element == null)
                {
                    throw new NotFoundException($"Element with id {elementId} not found.");
                }

                return JsonTranslator.ObjectToJson(element);
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Gets an element by its id.
        /// </summary>
        /// <param name="context">The HTTP Context of the request.</param>
        /// <param name="elementId">The id of the element to get.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [WebRouteMethod(Method = "GET", Path = "elements/:elementId/icon.png")]
        public async Task GetElementIcon(IHttpContext context, string elementId)
        {
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var element = this.Compendium.GetEntityById<Element>(elementId);
                if (element == null)
                {
                    throw new NotFoundException($"Element with id {elementId} not found.");
                }

                var sprite = ResourcesManager.GetSpriteForElement(element.Icon);
                return sprite.ToTexture().EncodeToPNG();
            });

            context.Response.Headers.Add("Content-Type", "image/png");
            await context.Response.WriteAllAsync(result);
        }
    }
}
