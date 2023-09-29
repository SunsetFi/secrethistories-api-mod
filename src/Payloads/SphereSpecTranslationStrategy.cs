namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for the <see cref="SphereSpec"/> class.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(SphereSpec))]
    public class SphereSpecTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the sphere spec.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The id.</returns>
        [JsonPropertyGetter("id")]
        public string GetId(SphereSpec sphereSpec)
        {
            return sphereSpec.Id;
        }

        /// <summary>
        /// Gets the label for this sphere spec.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(SphereSpec sphereSpec)
        {
            return sphereSpec.Label;
        }

        /// <summary>
        /// Gets the action id this slot should be active on.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The action id.</returns>
        [JsonPropertyGetter("actionId")]
        public string GetActionId(SphereSpec sphereSpec)
        {
            return sphereSpec.ActionId;
        }

        /// <summary>
        /// Gets the description for this sphere spec.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(SphereSpec sphereSpec)
        {
            return sphereSpec.Description;
        }

        /// <summary>
        /// Gets a value indicating whether this sphere spec is greedy.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>A value indicating if this sphere spec is greedy.</returns>
        [JsonPropertyGetter("greedy")]
        public bool GetGreedy(SphereSpec sphereSpec)
        {
            return sphereSpec.Greedy;
        }

        /// <summary>
        /// Gets aspects that must be present for a slot to exist.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The required aspects.</returns>
        [JsonPropertyGetter("ifAspectsPresent")]
        public Dictionary<string, string> GetIfAspectsPresent(SphereSpec sphereSpec)
        {
            return sphereSpec.IfAspectsPresent;
        }

        /// <summary>
        /// Gets aspects that all must be present for an element to slot into this sphere.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The essential aspects.</returns>
        [JsonPropertyGetter("essential")]
        public JObject GetEssential(SphereSpec sphereSpec)
        {
            var result = new JObject();
            foreach (var pair in sphereSpec.Essential)
            {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        /// <summary>
        /// Gets aspects, one of which must be present for an element to slot into this sphere.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The required aspects.</returns>
        [JsonPropertyGetter("required")]
        public JObject GetRequired(SphereSpec sphereSpec)
        {
            var result = new JObject();
            foreach (var pair in sphereSpec.Required)
            {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }

        /// <summary>
        /// Gets aspects, any one of which will prevent an element from slotting into this sphere.
        /// </summary>
        /// <param name="sphereSpec">The sphere spec.</param>
        /// <returns>The forbidden elements.</returns>
        [JsonPropertyGetter("forbidden")]
        public JObject GetForbidden(SphereSpec sphereSpec)
        {
            var result = new JObject();
            foreach (var pair in sphereSpec.Forbidden)
            {
                result.Add(pair.Key, pair.Value);
            }

            return result;
        }
    }
}
