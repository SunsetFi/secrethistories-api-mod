namespace SHRestAPI.Payloads
{
    using System.Linq;
    using HarmonyLib;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.Tokens.Payloads;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for connected terrains.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(WisdomNodeTerrain))]
    public class WisdomNodeTerrainTranslationStrategy
    {
        /// <summary>
        /// Gets recipe slotted into the wisdom node, if any.
        /// </summary>
        /// <param name="terrain">The wisdom node terrain.</param>
        /// <returns>The ID of the wisdom recipe, or null.</returns>
        [JsonPropertyGetter("wisdomRecipeId")]
        public string GetSlottedRecipeId(WisdomNodeTerrain terrain)
        {
            AspectsInContext aspects;

            var committedSphere = Traverse.Create(terrain).Field<CommitmentSphere>("commitmentSphere").Value;
            var inputSphere = Traverse.Create(terrain).Field<Sphere>("inputSphere").Value;
            if (committedSphere.Tokens.Any())
            {
                aspects = Watchman.Get<HornedAxe>().GetAspectsInContext(committedSphere);
            }
            else if (inputSphere.Tokens.Any())
            {
                aspects = Watchman.Get<HornedAxe>().GetAspectsInContext(inputSphere);
            }
            else
            {
                return null;
            }

            var recipe = Watchman.Get<Compendium>().GetFirstMatchingRecipe(aspects, terrain.Id);
            if (recipe == null || !recipe.IsValid())
            {
                return null;
            }

            return recipe.Id;
        }

        /// <summary>
        /// Gets recipe used to commit the wisdom node, if any.
        /// </summary>
        /// <param name="terrain">The wisdom node terrain.</param>
        /// <returns>True if the wisdom node is committed, false otherwise.</returns>
        [JsonPropertyGetter("committed")]
        public bool GetCommitted(WisdomNodeTerrain terrain)
        {
            var sphere = Traverse.Create(terrain).Field<CommitmentSphere>("commitmentSphere").Value;
            return sphere.Tokens.Any();
        }
    }
}
