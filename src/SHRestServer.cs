using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SHRestAPI;
using SHRestAPI.JsonTranslation;
using SHRestAPI.Server;
using SHRestAPI.Server.Attributes;

/// <summary>
/// This is the entry point for the SHRestAPI mod.
/// </summary>
public static class SHRestServer
{
    /// <summary>
    /// The web router instance.
    /// </summary>
    private static readonly WebRouter Router = new();

    /// <summary>
    /// The web server instance.
    /// </summary>
    private static WebServer webServer;

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
    /// Initialize the server.
    /// </summary>
    public static void Initialize()
    {
        Logging.LogInfo("SHRestServer Initializing.");
        try
        {
            var ownAssembly = typeof(SHRest).Assembly;
            JsonTranslator.LoadJsonTranslatorStrategies(ownAssembly);

            RegisterControllers(ownAssembly);
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to initialize SHRestAPI: {ex}");
            return;
        }

        try
        {
            StartServer();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to start SHRestAPI Server: {ex}");
        }
    }

    public static void RegisterControllers(Assembly assembly)
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
            Router.AddRoute(route);
        }

        Logging.LogTrace($"Loaded {controllerRoutes.Length} routes from {controllerTypes.Length} controllers in {assembly.FullName}");
    }

    private static void StartServer()
    {
        if (webServer != null)
        {
            return;
        }

        webServer = new WebServer(OnRequest);
        webServer.Start(8081);
    }

    private static async Task<bool> OnRequest(IHttpContext context)
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

        return await Router.HandleRequest(context);
    }
}
