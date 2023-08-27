namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json.Linq;
    using SecretHistories.Spheres;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translates between a <see cref="Sphere"/> and JSON.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Sphere))]
    public sealed class SphereTranslationStrategy
    {
        /// <summary>
        /// Gets the id of the sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere id.</returns>
        [JsonPropertyGetter("id")]
        public string GetId(Sphere sphere)
        {
            return sphere.Id;
        }

        /// <summary>
        /// Gets the path for the sphere.
        /// </summary>
        /// <param name="sphere">The sphere to get the path of.</param>
        /// <returns>The sphere path.</returns>
        [JsonPropertyGetter("path")]
        public string GetPath(Sphere sphere)
        {
            return sphere.GetAbsolutePath().Path;
        }

        /// <summary>
        /// Gets the category of the sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere category.</returns>
        [JsonPropertyGetter("category")]
        public string GetCategory(Sphere sphere)
        {
            return sphere.SphereCategory.ToString();
        }

        /// <summary>
        /// Gets a value indicating if the sphere is shrouded.
        /// </summary>
        /// <param name="sphere">The shere.</param>
        /// <returns>The sphere's shrouded status.</returns>
        [JsonPropertyGetter("shrouded")]
        public bool GetShrouded(Sphere sphere)
        {
            return sphere.Shrouded;
        }

        /// <summary>
        /// Gets the label of the sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(Sphere sphere)
        {
            return sphere.GoverningSphereSpec?.Label;
        }

        /// <summary>
        /// Gets the description of the sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(Sphere sphere)
        {
            return sphere.GoverningSphereSpec?.Description;
        }

#if CS
// This was in BH, but seems to have been removed in a beta.
        /// <summary>
        /// Gets a value indicating if the sphere will consume its card.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere's consumes status.</returns>
        [JsonPropertyGetter("consumes")]
        public bool GetConsumes(Sphere sphere)
        {
            return sphere.GoverningSphereSpec?.Consumes ?? false;
        }
#endif

        /// <summary>
        /// Gets a value indicating if the sphere is greedy.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The sphere's greedy status.</returns>
        [JsonPropertyGetter("greedy")]
        public bool GetGreedy(Sphere sphere)
        {
            return sphere.GoverningSphereSpec?.Greedy ?? false;
        }

        /// <summary>
        /// Gets the attributes that all must be present on a token to be inserted into this sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The essential attributes.</returns>
        [JsonPropertyGetter("essentialAspects")]
        public JObject GetEssentialAspects(Sphere sphere)
        {
            var spec = sphere.GoverningSphereSpec;
            if (spec == null)
            {
                return null;
            }

            var result = new JObject();
            foreach (var attribute in spec.Essential)
            {
                result[attribute.Key] = attribute.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets the attributes of which at least one must be present on a token to be inserted into this sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The required attributes.</returns>
        [JsonPropertyGetter("requiredAspects")]
        public JObject GetRequiredAspects(Sphere sphere)
        {
            var spec = sphere.GoverningSphereSpec;
            if (spec == null)
            {
                return null;
            }

            var result = new JObject();
            foreach (var attribute in spec.Required)
            {
                result[attribute.Key] = attribute.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets the attributes of which at none must be present on a token to be inserted into this sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>The forbidden attributes.</returns>
        [JsonPropertyGetter("forbiddenAspects")]
        public JObject GetForbiddenAspects(Sphere sphere)
        {
            var spec = sphere.GoverningSphereSpec;
            if (spec == null)
            {
                return null;
            }

            var result = new JObject();
            foreach (var attribute in spec.Forbidden)
            {
                result[attribute.Key] = attribute.Value;
            }

            return result;
        }
    }
}
