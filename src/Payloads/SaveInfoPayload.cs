namespace SHRestAPI.Payloads
{
    using System;
    using Newtonsoft.Json;
    using SecretHistories.Infrastructure.Persistence;

    /// <summary>
    /// Payload for describing a game save.
    /// </summary>
    public class SaveInfoPayload
    {
        /// <summary>
        /// Gets or sets the name of the save.
        /// </summary>
        [JsonProperty("saveName")]
        public string SaveName { get; set; }

        /// <summary>
        /// Gets or sets the date the save was created.
        /// </summary>
        [JsonProperty("saveDate")]
        public DateTime SaveDate { get; set; }

        /// <summary>
        /// Creates a new save info payload from a save info object.
        /// </summary>
        /// <param name="info">The save info to create the payload from.</param>
        /// <returns>The new save info payload.</returns>
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
