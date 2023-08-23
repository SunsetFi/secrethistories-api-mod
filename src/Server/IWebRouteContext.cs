namespace SHRestAPI.Server
{
    using System.Collections.Generic;
    using Ceen;

    /// <summary>
    /// Http Context for a WebAPI Route request.
    /// </summary>
    public interface IWebRouteContext : IHttpContext
    {
        /// <summary>
        /// Gets the path parameters for this request.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }
    }
}
