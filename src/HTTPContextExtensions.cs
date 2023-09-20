namespace SHRestAPI
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Ceen;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Extension methods for the IHTTPContext interface.
    /// </summary>
    public static class HTTPContextExtensions
    {
        /// <summary>
        /// Parse the body of the request as a JSON object and deserialize it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the body to.</typeparam>
        /// <param name="context">The context to deserialize.</param>
        /// <returns>The body of the request deserialized as json.</returns>
        public static T ParseBody<T>(this IHttpContext context)
        {
            var body = context.Request.Body;
            var reader = new StreamReader(body, System.Text.Encoding.UTF8);
            var text = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<T>(text);
        }

        /// <summary>
        /// Parse the body of the request as a JSON object and deserialize it to the specified type.
        /// </summary>
        /// <param name="context">The context to deserialize.</param>
        /// <param name="type">The type to which the body should be deserialized.</param>
        /// <returns>The body of the request deserialized to the specified type.</returns>
        public static object ParseBody(this IHttpContext context, Type type)
        {
            var body = context.Request.Body;
            var reader = new StreamReader(body, System.Text.Encoding.UTF8);
            var text = reader.ReadToEnd();

            return JsonConvert.DeserializeObject(text, type);
        }

        /// <summary>
        /// Parse the body of the request and return it as a JSON token.
        /// </summary>
        /// <param name="context">The context from which to parse the body.</param>
        /// <returns>The body of the request as a JToken.</returns>
        public static JToken ParseJson(this IHttpContext context)
        {
            var body = context.Request.Body;
            var reader = new StreamReader(body, System.Text.Encoding.UTF8);
            var text = reader.ReadToEnd();
            return JToken.Parse(text);
        }

        /// <summary>
        /// Send a response with the specified status code and a JSON body.
        /// </summary>
        /// <param name="context">The context to which to send the response.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="jsonBody">The object to serialize as JSON and include in the body of the response.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static Task SendResponse(this IHttpContext context, HttpStatusCode statusCode, object jsonBody)
        {
            var jsonText = JsonConvert.SerializeObject(jsonBody, Formatting.Indented);
            return SendResponse(context, statusCode, "application/json", jsonText);
        }

        /// <summary>
        /// Send a response with the specified status code, content type, and body.
        /// </summary>
        /// <param name="context">The context to which to send the response.</param>
        /// <param name="statusCode">The HTTP status code of the response.</param>
        /// <param name="contentType">The MIME type of the response's content.</param>
        /// <param name="body">The content to include in the body of the response.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task SendResponse(this IHttpContext context, HttpStatusCode statusCode, string contentType = null, string body = null)
        {
            var response = context.Response;

            response.StatusCode = statusCode;

            if (contentType != null)
            {
                response.Headers.Add("Content-Type", contentType);
            }

            if (contentType == "application/json")
            {
                await response.WriteAllJsonAsync(body);
            }
            else if (body != null)
            {
                await response.WriteAllAsync(body, contentType);
            }
        }

        /// <summary>
        /// Sends a file by path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the file to send.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        public static async Task SendFileResponse(this IHttpContext context, string path)
        {
            var mimeType = MimeMapper.GetMimeType(Path.GetExtension(path));
            var response = context.Response;

            response.StatusCode = HttpStatusCode.OK;
            response.Headers.Add("Content-Type", mimeType);

            await response.WriteAllAsync(File.ReadAllBytes(path), mimeType);
        }
    }
}
