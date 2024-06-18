namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Payload for loading a save.
    /// </summary>
    public class LoadSavePayload
    {
        /// <summary>
        /// Gets or sets the name of the save to load.
        /// </summary>
        [JsonProperty("saveName")]
        public string SaveName { get; set; }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.SaveName))
            {
                throw new BadRequestException("Save name is required.");
            }
        }
    }
}
