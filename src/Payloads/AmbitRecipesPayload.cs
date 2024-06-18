namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for reporting on ambit recipes.
    /// </summary>
    public class AmbitRecipesPayload
    {
        /// <summary>
        /// Gets or sets the open ambit recipe IDs.
        /// </summary>
        [JsonProperty("openAmbitRecipeIds")]
        public List<string> OpenAmbitRecipeIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the locked ambit recipe IDs.
        /// </summary>
        [JsonProperty("lockedAmbitRecipeIds")]
        public List<string> LockedAmbitRecipeIds { get; set; } = new();
    }
}
