namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Tokens.Payloads;
    using SHRestAPI.JsonTranslation;

    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(ConnectedTerrain))]
    public class ConnectedTerrainTranslationStrategy
    {
        [JsonPropertyGetter("unlockRequirements")]
        public IDictionary<string, int> GetUnlockRequirements(ConnectedTerrain terrain)
        {
            return terrain.RequiredForInputSphere.ToDictionary(aspect => aspect.name, aspect => aspect.level);
        }

        [JsonPropertyGetter("unlockForbiddens")]
        public IDictionary<string, int> GetForbiddenRequirements(ConnectedTerrain terrain)
        {
            return terrain.ForbiddenInInputSphere.ToDictionary(aspect => aspect.name, aspect => aspect.level);
        }

        [JsonPropertyGetter("unlockEssentials")]
        public IDictionary<string, int> GetEssentialRequirements(ConnectedTerrain terrain)
        {
            return terrain.EssentialInInputSphere.ToDictionary(aspect => aspect.name, aspect => aspect.level);
        }
    }
}
