namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AmbitRecipesPayload
    {
        [JsonProperty("openAmbitRecipeIds")]
        public List<string> openAmbitRecipeIds { get; set; } = new();

        [JsonProperty("lockedAmbitRecipeIds")]
        public List<string> lockedAmbitRecipeIds { get; set; } = new();
    }
}
