namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;

    /// <summary>
    /// Payload for a legacy response.
    /// </summary>
    public class SetRecipePayload
    {
        /// <summary>
        /// Gets or sets the legacy id.
        /// </summary>
        [JsonProperty("recipeId")]
        public string RecipeId { get; set; }
    }
}
