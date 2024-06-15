namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json;
    using SecretHistories.Enums;

    /// <summary>
    /// Payload for a legacy response.
    /// </summary>
    public class SetRecipePayload
    {
        /// <summary>
        /// Gets or sets the situation state required to set the recipe.
        /// </summary>
        /// <value>The state required to set the recipe.</value>
        /// <remarks>Optional.  This can be used to ensure only an unstarted recipe is affected.</remarks>
        [JsonProperty("requireState")]
        public StateEnum? RequireState { get; set; }

        /// <summary>
        /// Gets or sets the legacy id.
        /// </summary>
        [JsonProperty("recipeId")]
        public string RecipeId { get; set; }
    }
}
