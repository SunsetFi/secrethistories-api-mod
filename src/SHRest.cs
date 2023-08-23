using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ceen;
using SHRestAPI;
using SHRestAPI.JsonTranslation;
using SHRestAPI.Payloads;
using SHRestAPI.Server;
using SHRestAPI.Server.Attributes;
using SHRestAPI.Server.Exceptions;
using UnityEngine;

/// <summary>
/// This is the entry point for the SHRestAPI mod.
/// </summary>
public class SHRest : MonoBehaviour
{
    /// <summary>
    /// The web router instance.
    /// </summary>
    private readonly WebRouter router = new();

    /// <summary>
    /// The web server instance.
    /// </summary>
    private WebServer webServer;

    /// <summary>
    /// Initialize the mod.
    /// </summary>
    public static void Initialise()
    {
        try
        {
            new GameObject().AddComponent<SHRest>();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to initialize SHRestAPI: {ex}");
        }
    }

    /// <summary>
    /// Unity callback for when the game object is created.
    /// This registers our controllers and starts the server.
    /// </summary>
    public void Start()
    {
        try
        {
            Dispatcher.Initialize();

            var ownAssembly = typeof(SHRest).Assembly;
            JsonTranslator.LoadJsonTranslatorStrategies(ownAssembly);

            this.RegisterControllers(ownAssembly);
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to start SHRestAPI: {ex}");
            return;
        }

        try
        {
            this.StartServer();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to start CSRest Server: {ex}");
        }
    }

    private void RegisterControllers(Assembly assembly)
    {
        var controllerTypes = (from type in assembly.GetTypes()
                               where type.GetCustomAttribute(typeof(WebControllerAttribute)) != null
                               select type).ToArray();

        var controllerRoutes = (from type in controllerTypes
                                let controller = Activator.CreateInstance(type)
                                let routes = WebControllerFactory.CreateRoutesFromController(controller)
                                from route in routes
                                select route).ToArray();

        foreach (var route in controllerRoutes)
        {
            this.router.AddRoute(route);
        }

        Logging.LogTrace($"Loaded {controllerRoutes.Length} routes from {controllerTypes.Length} controllers in {assembly.FullName}");
    }

    private void StartServer()
    {
        if (this.webServer != null)
        {
            return;
        }

        this.webServer = new WebServer(this.OnRequest);
        this.webServer.Start(8081);
    }

    private async Task<bool> OnRequest(IHttpContext context)
    {
        if (context.Request.Method == "OPTIONS")
        {
            // For a proper implementation of CORS, see https://github.com/expressjs/cors/blob/master/lib/index.js#L159
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");

            // TODO: Choose based on available routes at this path
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, PATCH, POST, DELETE");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            context.Response.AddHeader("Access-Control-Max-Age", "1728000");
            context.Response.StatusCode = HttpStatusCode.NoContent;
            context.Response.Headers["Content-Length"] = "0";
            return true;
        }
        else
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
        }

        try
        {
            return await this.router.HandleRequest(context);
        }
        catch (Exception e)
        {
            var webException = e.GetInnerException<WebException>();
            if (webException != null)
            {
                Logging.LogError(
                    new Dictionary<string, string>()
                    {
                        { "METHOD", context.Request.Method },
                        { "PATH", context.Request.Path },
                        { "STATUS", webException.StatusCode.ToString() },
                    }, $"Failed to handle request: {webException.Message}\n{webException.StackTrace}");

                await context.SendResponse(webException.StatusCode, new ErrorPayload
                {
                    Message = webException.Message,
                });
            }
            else
            {
                Logging.LogError(
                    new Dictionary<string, string>()
                    {
                    { "METHOD", context.Request.Method },
                    { "PATH", context.Request.Path },
                    }, $"Failed to handle request: {e}\n{e.StackTrace}");

                await context.SendResponse(HttpStatusCode.InternalServerError, new ErrorPayload
                {
                    Message = e.Message,
                });
            }

            return true;
        }
    }
}
