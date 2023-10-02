namespace SHRestAPI.Server
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides an HTTP Context from an HttpListener context.
    /// </summary>
    public class HttpListenerHttpContext : IHttpContext
    {
        private readonly HttpListenerContext context;
        private readonly IReadOnlyDictionary<string, string> queryString;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpListenerHttpContext"/> class.
        /// </summary>
        /// <param name="context">The context to wrap.</param>
        public HttpListenerHttpContext(HttpListenerContext context)
        {
            this.context = context;
            this.queryString = context.Request.QueryString.AllKeys.Where(x => x != null).ToDictionary(key => key, key => context.Request.QueryString[key]);
        }

        /// <inheritdoc/>
        public string Method => this.context.Request.HttpMethod;

        /// <inheritdoc/>
        public string Path => this.context.Request.Url.LocalPath;

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> QueryString => this.queryString;

        /// <inheritdoc/>
        public Stream Body => this.context.Request.InputStream;

        /// <inheritdoc/>
        public void SetHeader(string header, string value)
        {
            this.context.Response.Headers.Add(header, value);
        }

        /// <inheritdoc/>
        public Task SendResponse(HttpStatusCode statusCode)
        {
            this.context.Response.StatusCode = (int)statusCode;
            this.context.Response.Headers.Add("Content-Length", "0");

            this.Dispose();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SendResponse(HttpStatusCode statusCode, string contentType, Stream response)
        {
            this.context.Response.StatusCode = (int)statusCode;
            this.context.Response.ContentType = contentType;

            response.CopyTo(this.context.Response.OutputStream);

            this.Dispose();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.context.Response.Close();
                this.isDisposed = true;
            }
        }
    }
}
