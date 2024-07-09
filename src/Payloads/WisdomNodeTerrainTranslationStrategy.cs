namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
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
        /// Gets the required unlock aspects for the wisdom skill.
        /// </summary>
        [JsonPropertyGetter("wisdomSkillRequirements")]
        public IDictionary<string, int> GetEssentialSkillRequirements(WisdomNodeTerrain terrain)
        {
            var spec = Traverse.Create(terrain).Field<Sphere>("inputSphere").Value.GoverningSphereSpec;
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Required.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        /// <summary>
        /// Gets the forbidden unlock aspects for the wisdom skill.
        /// </summary>
        [JsonPropertyGetter("wisdomSkillForbiddens")]
        public IDictionary<string, int> GetWisdomSkillForbiddens(WisdomNodeTerrain terrain)
        {
            var spec = Traverse.Create(terrain).Field<Sphere>("inputSphere").Value.GoverningSphereSpec;
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Forbidden.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        /// <summary>
        /// Gets the essential unlock aspects for the wisdom skill.
        /// </summary>
        [JsonPropertyGetter("wisdomSkillEssentials")]
        public IDictionary<string, int> GetWisdomSkillEssentials(WisdomNodeTerrain terrain)
        {
            var spec = Traverse.Create(terrain).Field<Sphere>("inputSphere").Value.GoverningSphereSpec;
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Essential.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

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
                aspects = ThreadSafeHornedAxe.GetAspectsInContext(committedSphere);
            }
            else if (inputSphere.Tokens.Any())
            {
                aspects = ThreadSafeHornedAxe.GetAspectsInContext(inputSphere);
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
