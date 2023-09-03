namespace SHRestAPI.Payloads
{
    using SecretHistories.Entities;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for the <see cref="SphereSpec"/> class.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(SphereSpec))]
    public class SphereSpecTranslationStrategy
    {
        // TODO: SphereSpec json.
    }
}
