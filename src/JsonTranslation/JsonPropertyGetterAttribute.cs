namespace SHRestAPI.JsonTranslation
{
    using System;

    /// <summary>
    /// Mark a function as implementing a property getter for an auto translation strategy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class JsonPropertyGetterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyGetterAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The name of the property to act as a getter for.</param>
        public JsonPropertyGetterAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the property that this function gets.
        /// </summary>
        public string PropertyName { get; }
    }
}
