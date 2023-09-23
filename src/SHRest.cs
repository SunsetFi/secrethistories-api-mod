using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SHRestAPI;
using SHRestAPI.JsonTranslation;
using SHRestAPI.Server;
using SHRestAPI.Server.Attributes;
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
    /// Gets the path to the web host content.
    /// </summary>
    public static string WebhostPath
    {
        get
        {
            return Path.Combine(Path.GetDirectoryName(typeof(SHRest).Assembly.Location), "web-content");
        }
    }

    /// <summary>
    /// Initialize the mod.
    /// </summary>
    public static void Initialise()
    {
        Logging.LogInfo("SHRestAPI Initializing.");
        try
        {
            var go = new GameObject();
            go.AddComponent<SHRest>();
            DontDestroyOnLoad(go);
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
    public void Awake()
    {
        Logging.LogInfo("SHRestAPI Initializing (Return Of Khan).");
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
                               let attr = (WebControllerAttribute)type.GetCustomAttribute(typeof(WebControllerAttribute))
                               where attr != null
                               orderby attr.Priority descending
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
        if (context.Method == "OPTIONS")
        {
            // For a proper implementation of CORS, see https://github.com/expressjs/cors/blob/master/lib/index.js#L159
            context.SetHeader("Access-Control-Allow-Origin", "*");

            // TODO: Choose based on available routes at this path
            context.SetHeader("Access-Control-Allow-Methods", "GET, PUT, PATCH, POST, DELETE");
            context.SetHeader("Access-Control-Allow-Headers", "Content-Type");
            context.SetHeader("Access-Control-Max-Age", "1728000");
            await context.SendResponse(HttpStatusCode.NoContent);
            return true;
        }
        else
        {
            context.SetHeader("Access-Control-Allow-Origin", "*");
        }

        return await this.router.HandleRequest(context);
    }
}
