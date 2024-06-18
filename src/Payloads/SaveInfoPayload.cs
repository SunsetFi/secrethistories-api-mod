namespace SHRestAPI.Payloads
{
    using System;
    using Newtonsoft.Json;
    using SecretHistories.Infrastructure.Persistence;

    class SaveInfoPayload
    {
        [JsonProperty("saveName")]
        public string SaveName { get; set; }

        [JsonProperty("saveDate")]
        public DateTime SaveDate { get; set; }

        public static SaveInfoPayload FromInfo(SaveInfo info)
        {
            return new SaveInfoPayload
            {
                SaveName = info.Name,
                SaveDate = info.Date,
            };
        }
    }
}
