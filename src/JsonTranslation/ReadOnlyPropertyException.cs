namespace SHRestAPI.JsonTranslation
{
    /// <summary>
    /// Exception thrown when a translation to or from JSON fails due to a property being read-only.
    /// </summary>
    public class ReadOnlyPropertyException : JsonTranslationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyPropertyException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ReadOnlyPropertyException(string message)
            : base(message)
        {
        }
    }
}
