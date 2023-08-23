namespace SHRestAPI.JsonTranslation
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Defines a translator strategy for converting a type to or from a JSON object.
    /// </summary>
    public interface IJsonTranslatorStrategy
    {
        /// <summary>
        /// Gets the type that this strategy can translate.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets the names of the properties that this strategy can translate.
        /// </summary>
        IReadOnlyCollection<string> SupportedProperties { get; }

        /// <summary>
        /// Writes the given object to json.
        /// </summary>
        /// <param name="target">The object to write.</param>
        /// <param name="output">The JSON object to write to.</param>
        void WriteObjectToJson(object target, JObject output);

        /// <summary>
        /// Verify that the given input JSON object can be written to the target.
        /// </summary>
        /// <param name="target">The target to write to.</param>
        /// <param name="input">The JSON object to test against.</param>
        void VerifyJsonUpdate(object target, JObject input);

        /// <summary>
        /// Updates the given object from the given JSON object.
        /// </summary>
        /// <param name="target">The target to write to.</param>
        /// <param name="input">The JSON object to write to the target.</param>
        void UpdateObjectFromJson(object target, JObject input);
    }
}
