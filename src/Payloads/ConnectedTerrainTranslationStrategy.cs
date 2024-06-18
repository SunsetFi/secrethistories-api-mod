namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Tokens.Payloads;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for connected terrains.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(ConnectedTerrain))]
    public class ConnectedTerrainTranslationStrategy
    {
        /// <summary>
        /// Gets the required unlock aspects for the connected terrain.
        /// </summary>
        [JsonPropertyGetter("unlockRequirements")]
        public IDictionary<string, int> GetUnlockRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe()?.PreSlots?.FirstOrDefault();
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Required.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        /// <summary>
        /// Gets the forbidden unlock aspects for the connected terrain.
        /// </summary>
        [JsonPropertyGetter("unlockForbiddens")]
        public IDictionary<string, int> GetForbiddenRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe()?.PreSlots?.FirstOrDefault();
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Forbidden.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        /// <summary>
        /// Gets the essential unlock aspects for the connected terrain.
        /// </summary>
        [JsonPropertyGetter("unlockEssentials")]
        public IDictionary<string, int> GetEssentialRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe()?.PreSlots?.FirstOrDefault();
            if (spec == null)
            {
                return new Dictionary<string, int>();
            }

            return spec.Essential.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }
    }
}
