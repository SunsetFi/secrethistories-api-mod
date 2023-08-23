namespace SHRestAPI.JsonTranslation
{
    using System;

    /// <summary>
    /// Mark a function as implementing a property setter for an auto translation strategy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonPropertySetterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertySetterAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to act as a setter for.</param>
        public JsonPropertySetterAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the property to act as a setter for.
        /// </summary>
        public string PropertyName { get; }
    }
}
