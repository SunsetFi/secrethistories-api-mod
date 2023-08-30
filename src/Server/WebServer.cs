namespace SHRestAPI.Server
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Ceen;
    using Ceen.Httpd;

    /// <summary>
    /// A web server for receiving HTTP requests.
    /// </summary>
    public class WebServer : IDisposable
    {
        private readonly HttpHandlerDelegate requestHandler;
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServer"/> class.
        /// </summary>
        /// <param name="requestHandler">The web request handler to use.</param>
        public WebServer(HttpHandlerDelegate requestHandler)
        {
            this.requestHandler = requestHandler;
        }

        /// <summary>
        /// Starts the HTTP server.
        /// </summary>
        /// <param name="port">The port to listen on.</param>
        public void Start(int port)
        {
            Logging.LogTrace("Starting web server");

            var tcs = new CancellationTokenSource();
            var config = new ServerConfig()
                .AddLogger(this.OnLogMessage)
                .AddRoute(this.OnRequest);
            config.MaxActiveRequests = 200;
            config.SocketBacklog = 200;
            config.KeepAliveMaxRequests = 200;
            config.KeepAliveTimeoutSeconds = 10;

            this.cancellationTokenSource = new CancellationTokenSource();
            HttpServer.ListenAsync(
                new IPEndPoint(IPAddress.Any, port),
                false,
                config,
                this.cancellationTokenSource.Token);

            Logging.LogInfo($"Server started on port {port}");
        }

        /// <summary>
        /// Stops the HTTP server.
        /// </summary>
        public void Dispose()
        {
            this.cancellationTokenSource.Cancel();
            Logging.LogTrace("Server stopped");
        }

        private async Task<bool> OnRequest(IHttpContext context)
        {
            try
            {
                return await this.requestHandler(context);
            }
            catch (Exception e)
            {
                var webException = e.GetInnerException<Exceptions.WebException>();
                if (webException != null)
                {
                    await context.SendResponse(webException.StatusCode, "text/plain", webException.Message);
                    return true;
                }

                throw;
            }
        }

        private async Task OnLogMessage(IHttpContext context, Exception exception, DateTime started, TimeSpan duration)
        {
            if (exception != null)
            {
                await Dispatcher.RunOnMainThread(() =>
                {
                    var aggregateException = exception as AggregateException;
                    if (aggregateException != null)
                    {
                        foreach (var ex in aggregateException.InnerExceptions)
                        {
                            this.LogException(ex, context);
                        }
                    }
                    else
                    {
                        this.LogException(exception, context);
                    }
                });
            }
        }

        private void LogException(Exception exception, IHttpContext context)
        {
            Logging.LogError(
                new Dictionary<string, string>()
                {
                    { "RequestMethod", context.Request.Method },
                    { "RequestPath", context.Request.Path },
                    { "RemoteEndpoint", context.Request.RemoteEndPoint.ToString() },
                },
                exception.ToString());
        }
    }
}
