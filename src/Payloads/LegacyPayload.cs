namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for a legacy response.
    /// </summary>
    public class LegacyPayload
    {
        /// <summary>
        /// Gets or sets the legacy id.
        /// </summary>
        [JsonProperty("legacyId")]
        public string LegacyId { get; set; }

        /// <summary>
        /// Gets or sets the legacy label.
        /// </summary>
        [JsonProperty("legacyLabel")]
        public string LegacyLabel { get; set; }
    }
}
