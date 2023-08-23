namespace SHRestAPI.JsonTranslation
{
    using System;

    /// <summary>
    /// Mark the class as a JSON translator for the specified type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JsonTranslatorTargetAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTranslatorTargetAttribute"/> class.
        /// </summary>
        /// <param name="targetType">The type to mark this class as atranslating.</param>
        public JsonTranslatorTargetAttribute(Type targetType)
        {
            this.TargetType = targetType;
        }

        /// <summary>
        /// Gets the type this class translates.
        /// </summary>
        public Type TargetType { get; }
    }
}
