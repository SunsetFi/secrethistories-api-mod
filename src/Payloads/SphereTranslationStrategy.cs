namespace SHRestAPI.Payloads
{
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
    }
}
