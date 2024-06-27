namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for a save creation request.
    /// </summary>
    public class CreateSavePayload
    {
        /// <summary>
        /// Gets or sets the save name.
        /// </summary>
        [JsonProperty("saveName")]
        public string SaveName { get; set; }
    }
}
