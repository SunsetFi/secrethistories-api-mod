namespace SHRestAPI.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Ceen;

    /// <summary>
    /// Implements a IWebRouteContext based on an IHttpContext.
    /// </summary>
    public class WebRouteContext : IWebRouteContext
    {
        private IHttpContext httpContext;
        private IDictionary<string, string> pathParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebRouteContext"/> class.
        /// </summary>
        /// <param name="httpContext">The backing IHttpContext.</param>
        /// <param name="pathParameters">The path parameters for this context.</param>
        public WebRouteContext(IHttpContext httpContext, IDictionary<string, string> pathParameters)
        {
            this.httpContext = httpContext;
            this.pathParameters = pathParameters;
        }

        /// <inheritdoc/>
        public IDictionary<string, string> PathParameters
        {
            get
            {
                return this.pathParameters;
            }

            set
            {
                this.pathParameters = value;
            }
        }

        /// <inheritdoc/>
        public IHttpRequest Request => this.httpContext.Request;

        /// <inheritdoc/>
        public IHttpResponse Response => this.httpContext.Response;

        /// <inheritdoc/>
        public IStorageCreator Storage => this.httpContext.Storage;

        /// <inheritdoc/>
        public IDictionary<string, string> Session { get => this.httpContext.Session; set => this.httpContext.Session = value; }

        /// <inheritdoc/>
        public IDictionary<string, string> LogData => this.httpContext.LogData;

        /// <inheritdoc/>
        public ILoadedModuleInfo LoadedModules => this.httpContext.LoadedModules;

        /// <inheritdoc/>
        public Task LogMessageAsync(LogLevel level, string message, Exception ex)
        {
            return this.httpContext.LogMessageAsync(level, message, ex);
        }
    }
}
