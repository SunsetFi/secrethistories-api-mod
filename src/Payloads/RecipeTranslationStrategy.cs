namespace SHRestAPI.Payloads
{
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// JSON translation strategies for <see cref="Recipe"/> entities.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Recipe))]
    public class RecipeTranslationStrategy
    {
        [JsonPropertyGetter("id")]
        public string GetId(Recipe recipe)
        {
            return recipe.Id;
        }

        [JsonPropertyGetter("label")]
        public string GetLabel(Recipe recipe)
        {
            return recipe.Label;
        }

        [JsonPropertyGetter("description")]
        public string GetDescription(Recipe recipe)
        {
#if BH
            return recipe.Desc;
#else
            return recipe.Description;
#endif
        }

        [JsonPropertyGetter("aspects")]
        public JObject GetAspects(Recipe recipe)
        {
            var aspects = new JObject();
            foreach (var aspect in recipe.Aspects)
            {
                aspects.Add(aspect.Key, aspect.Value);
            }

            return aspects;
        }

        [JsonPropertyGetter("startLabel")]
        public string GetStartLabel(Recipe recipe)
        {
            return recipe.StartLabel;
        }

        [JsonPropertyGetter("startDescription")]
        public string GetStartDescription(Recipe recipe)
        {
            return recipe.StartDescription;
        }

        [JsonPropertyGetter("preSlots")]
        public JObject[] GetPreSlots(Recipe recipe)
        {
            return recipe.PreSlots.Select(JsonTranslator.ObjectToJson).ToArray();
        }

        [JsonPropertyGetter("slots")]
        public JObject[] GetSlots(Recipe recipe)
        {
            return recipe.Slots.Select(JsonTranslator.ObjectToJson).ToArray();
        }
    }
}
