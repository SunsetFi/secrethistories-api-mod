namespace SHRestAPI.Controllers
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Ceen;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;
    using UnityEngine;

    /// <summary>
    /// A controller for providing hosted web content.
    /// </summary>
    [WebController(Path = "/", Priority = -1000)]
    public class WebhostController
    {
        private readonly string favIconElement = "readable";

        /// <summary>
        /// Gets the favicon for hosted content.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "favicon.png")]
        public async Task GetFavIcon(IHttpContext context)
        {
            // TODO: We really want an ICO here as that is what everything uses by default.
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                var element = Watchman.Get<Compendium>().GetEntityById<Element>(this.favIconElement);
                if (element == null || !element.IsValid())
                {
                    throw new NotFoundException($"Element with id {this.favIconElement} not found.");
                }

                var sprite = ResourcesManager.GetAppropriateSpriteForElement(element);
                return sprite.ToTexture().EncodeToPNG();
            });

            context.Response.Headers.Add("Content-Type", "image/png");
            await context.Response.WriteAllAsync(result);
        }

        /// <summary>
        /// Gets the index of the web host.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that is resolved when the request is completed.</returns>
        [WebRouteMethod(Method = "GET")]
        public Task GetIndex(IHttpContext context)
        {
            return this.SendDirectoryContent(context, SHRest.WebhostPath);
        }

        /// <summary>
        /// Gets the content of the given path from the web host.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="path">The request path.</param>
        /// <returns>A task that is resolved when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "**path")]
        public Task GetPath(IHttpContext context, string path)
        {
            // This is a bit hackish, but we dont want to give false results for anything that is actually our api.
            // Might want to find a more elegant solution here, as this isnt pluggable by other plugins.
            if (path.StartsWith("api"))
            {
                throw new NotFoundException();
            }

            path = this.NormalizeValidatePath(path);
            if (File.Exists(path))
            {
                return this.SendFile(context, path);
            }
            else if (Directory.Exists(path))
            {
                return this.SendDirectoryContent(context, path);
            }
            else
            {
                throw new NotFoundException();
            }
        }

        private Task SendFile(IHttpContext context, string path)
        {
            return context.SendFileResponse(path);
        }

        private Task SendDirectoryContent(IHttpContext context, string path)
        {
            if (File.Exists(Path.Combine(path, "index.html")))
            {
                return this.SendFile(context, Path.Combine(path, "index.html"));
            }

            if (File.Exists(Path.Combine(path, "index.htm")))
            {
                return this.SendFile(context, Path.Combine(path, "index.htm"));
            }

            var body = new StringBuilder();
            body.Append("""<html><head><link rel="icon" type="image/png" href="/favicon.png"></head><body><ul>""");

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var name = Path.GetFileName(directory);
                body.AppendLine($"<li><a href=\"{name}/\">{name}</a></li>");
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var name = Path.GetFileName(file);
                body.AppendLine($"<li><a href=\"{name}\">{name}</a></li>");
            }

            body.AppendLine("</ul></body></html>");

            return context.SendResponse(HttpStatusCode.OK, "text/html", body.ToString());
        }

        private string NormalizeValidatePath(string path)
        {
            if (path == null)
            {
                throw new NotFoundException();
            }

            var webhostPath = Path.GetFullPath(SHRest.WebhostPath);
            if (!webhostPath.EndsWith("\\"))
            {
                webhostPath += "\\";
            }

            if (path.StartsWith("/") || path.StartsWith("\\"))
            {
                path = path.Substring(1);
            }

            path = Path.GetFullPath(Path.Combine(webhostPath, path));

            Logging.LogInfo($"Normalized path {path}, webhost path {webhostPath}");

            if (!path.StartsWith(webhostPath))
            {
                throw new NotFoundException();
            }

            return path;
        }
    }
}
