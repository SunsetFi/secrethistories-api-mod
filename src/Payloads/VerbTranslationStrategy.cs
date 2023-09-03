namespace SHRestAPI.Payloads
{
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for the <see cref="Verb"/> class.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Verb))]
    public class VerbTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The id of the verb.</returns>
        [JsonPropertyGetter("id")]
        public string GetId(Verb verb)
        {
            return verb.Id;
        }

        /// <summary>
        /// Gets the label of the verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The label of the verb.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(Verb verb)
        {
            return verb.Label;
        }

        /// <summary>
        /// Gets the description of the verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The description of the verb.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(Verb verb)
        {
#if BH
            return verb.Desc;
#else
            return verb.Description;
#endif
        }

        /// <summary>
        /// Gets the icon of the verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The icon of the verb.</returns>
        [JsonPropertyGetter("icon")]
        public string GetIcon(Verb verb)
        {
            return verb.Icon;
        }

        /// <summary>
        /// Gets the category of the verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The category of the verb.</returns>
        [JsonPropertyGetter("category")]
        public string GetCategory(Verb verb)
        {
            return verb.Category.ToString();
        }

        /// <summary>
        /// Gets a value indicating if multiple copies of the verb can exist.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>A value indicating if multiple copies of the verb can exist.</returns>
        [JsonPropertyGetter("multiple")]
        public bool GetMultiple(Verb verb)
        {
            return verb.Multiple;
        }

        /// <summary>
        /// Gets a value indicating if the situation should dissipate after the recipes complete.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The value.</returns>
        [JsonPropertyGetter("spontaneous")]
        public bool GetSpontaneous(Verb verb)
        {
            return verb.Spontaneous;
        }

        /// <summary>
        /// Gets all input thresholds of this verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The input thresholds.</returns>
        [JsonPropertyGetter("thresholds")]
        public JObject[] GetThresholds(Verb verb)
        {
            return verb.Thresholds.Select(JsonTranslator.ObjectToJson).ToArray();
        }

        /// <summary>
        /// Gets all aspects native to this verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The verb aspects.</returns>
        [JsonPropertyGetter("aspects")]
        public JObject GetAspects(Verb verb)
        {
            var value = new JObject();
            foreach (var pair in verb.Aspects)
            {
                value.Add(pair.Key, pair.Value);
            }

            return value;
        }

        [JsonPropertyGetter("hints")]
        public string[] GetHints(Verb verb)
        {
            return verb.Hints.ToArray();
        }

        /// <summary>
        /// Gets the xtriggers for this verb.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <returns>The xtriggers.</returns>
        [JsonPropertyGetter("xtriggers")]
        public JObject GetXTriggers(Verb verb)
        {
            var obj = new JObject();
            foreach (var pair in verb.XTriggers)
            {
                // TODO: payload translator for MorphDetails
                obj[pair.Key] = new JArray(pair.Value.Select(effect =>
                {
                    return new JObject
                    {
                        ["id"] = effect.Id,
                        ["morpheffect"] = effect.MorphEffect.ToString().ToLower(),
                        ["level"] = effect.Level,
                        ["chance"] = effect.Chance,
                        ["lever"] = effect.Lever,
                    };
                }));
            }

            return obj;
        }
    }
}
