namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for error messages.
    /// </summary>
    public class ErrorPayload
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
