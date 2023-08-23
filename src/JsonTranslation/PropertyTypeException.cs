namespace SHRestAPI.JsonTranslation
{
    /// <summary>
    /// Exception thrown when a translation to or from JSON fails due to an incorrect property type.
    /// </summary>
    public class PropertyTypeException : JsonTranslationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PropertyTypeException(string message)
            : base(message)
        {
        }
    }
}
