namespace SHRestAPI.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Http Context for a WebAPI Route request.
    /// </summary>
    public interface IHttpContext : IDisposable
    {
        /// <summary>
        /// Gets the request method.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets the request path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        IReadOnlyDictionary<string, string> QueryString { get; }

        /// <summary>
        /// Gets the body stream.
        /// </summary>
        Stream Body { get; }

        /// <summary>
        /// Sets a header on the response.
        /// </summary>
        /// <param name="header">The header name.</param>
        /// <param name="value">The header value.</param>
        void SetHeader(string header, string value);

        /// <summary>
        /// Sends a response to the quest.
        /// </summary>
        /// <param name="statusCode">The response status code.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task SendResponse(HttpStatusCode statusCode);

        /// <summary>
        /// Sends a response to the request.
        /// </summary>
        /// <param name="statusCode">The response status code.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="response">The response body.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task SendResponse(HttpStatusCode statusCode, string contentType, Stream response);
    }
}
