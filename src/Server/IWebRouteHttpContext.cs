namespace SHRestAPI.Server
{
    using System.Collections.Generic;

    /// <summary>
    /// Http Context for a WebAPI Route request.
    /// </summary>
    public interface IWebRouteHttpContext : IHttpContext
    {
        /// <summary>
        /// Gets the path parameters for this request.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }
    }
}
