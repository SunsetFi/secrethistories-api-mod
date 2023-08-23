namespace SHRestAPI.Server.Attributes
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Attribute to mark a method as being middleware.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class WebMiddlewareAttribute : Attribute
    {
        /// <summary>
        ///  Initializes a new instance of the <see cref="WebMiddlewareAttribute"/> class.
        /// </summary>
        /// <param name="handler">The function to implement the middleware.</param>
        public WebMiddlewareAttribute(Func<IWebRouteContext, Task<bool>> handler)
        {
            this.Handler = handler;
        }

        /// <summary>
        /// Gets the handler for the middleware.
        /// </summary>
        public Func<IWebRouteContext, Task<bool>> Handler { get; private set; }
    }
}
