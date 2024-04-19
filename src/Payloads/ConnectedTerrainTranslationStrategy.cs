namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Tokens.Payloads;
    using SHRestAPI.JsonTranslation;

    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(ConnectedTerrain))]
    public class ConnectedTerrainTranslationStrategy
    {
        [JsonPropertyGetter("unlockRequirements")]
        public IDictionary<string, int> GetUnlockRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe().PreSlots.FirstOrDefault();
            return spec.Required.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        [JsonPropertyGetter("unlockForbiddens")]
        public IDictionary<string, int> GetForbiddenRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe().PreSlots.FirstOrDefault();
            return spec.Forbidden.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }

        [JsonPropertyGetter("unlockEssentials")]
        public IDictionary<string, int> GetEssentialRequirements(ConnectedTerrain terrain)
        {
            var spec = terrain.GetInfoRecipe().PreSlots.FirstOrDefault();
            return spec.Essential.ToDictionary(aspect => aspect.Key, aspect => aspect.Value);
        }
    }
}
