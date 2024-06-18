namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;
    using SHRestAPI.Server.Exceptions;

    class LoadSavePayload
    {
        [JsonProperty("saveName")]
        public string SaveName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.SaveName))
            {
                throw new BadRequestException("Save name is required.");
            }
        }
    }
}