#if BH
namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using SecretHistories.Tokens.Payloads;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// JSON translation strategies for TerrainFeatures.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(TerrainFeature))]
    public class TerrainFeatureTranslationStrategy
    {
        /// <summary>
        /// Gets the label of this terrain feature.
        /// </summary>
        /// <param name="terrain">The terrain feature.</param>
        /// <returns>The label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(TerrainFeature terrain)
        {
            return terrain.Label;
        }

        /// <summary>
        /// Gets the description of this terrain feature.
        /// </summary>
        /// <param name="terrain">The terrain feature.</param>
        /// <returns>The description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(TerrainFeature terrain)
        {
            return terrain.Description;
        }

        /// <summary>
        /// Gets the aspects for this terrain feature.
        /// </summary>
        /// <param name="terrain">The terrain feature.</param>
        /// <returns>The aspects.</returns>
        [JsonPropertyGetter("aspects")]
        public Dictionary<string, int> GetAspects(TerrainFeature terrain)
        {
            // These are glued to the TerrainFeature and given through this function call.
            // InfoRecipe.Aspects is not involved.
            return terrain.GetAspects(false);
        }

        /// <summary>
        /// Gets a value indicating if the terrain is sealed.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <returns>A value indicating if the terrain is sealed.</returns>
        [JsonPropertyGetter("sealed")]
        public bool GetSealed(TerrainFeature terrain)
        {
            return terrain.IsSealed;
        }

        /// <summary>
        /// Sets whether the terrain is sealed.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="value">A value indicating if the terrain should be sealed.</param>
        [JsonPropertySetter("sealed")]
        public void SetSealed(TerrainFeature terrain, bool value)
        {
            if (value)
            {
                terrain.Seal();
            }
            else
            {
                terrain.Unseal();
            }
        }

        /// <summary>
        /// Gets a value indicating if the terrain is shrouded.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <returns>A value indicating if the terrain is shrouded.</returns>
        [JsonPropertyGetter("shrouded")]
        public bool GetShrouded(TerrainFeature terrain)
        {
            return terrain.IsShrouded;
        }

        /// <summary>
        /// Sets whether the terrain is shrouded.
        /// </summary>
        /// <param name="terrain">The terrain.</param>
        /// <param name="value">A value indicating if the terrain should be shrouded.</param>
        [JsonPropertySetter("shrouded")]
        public void SetShrouded(TerrainFeature terrain, bool value)
        {
            if (value)
            {
                terrain.Shroud(true);
            }
            else
            {
                terrain.Unshroud(true);
            }
        }

        /// <summary>
        /// Gets the info recipe for this terrain feature.
        /// </summary>
        /// <param name="terrain">The terrain feature.</param>
        /// <returns>The info recipe.</returns>
        [JsonPropertyGetter("infoRecipeId")]
        public string GetInfoRecipe(TerrainFeature terrain)
        {
            return terrain.GetInfoRecipe()?.Id;
        }

        // [JsonPropertyGetter("infoRecipeStartLabel")]
        // public string GetInfoRecipeStartLabel(TerrainFeature terrain)
        // {
        //     return terrain.InfoRecipe?.StartLabel;
        // }

        // [JsonPropertyGetter("infoRecipeStartDescription")]
        // public string GetInfoRecipeStartDescription(TerrainFeature terrain)
        // {
        //     return terrain.InfoRecipe?.StartDescription;
        // }
    }
}
#endif
