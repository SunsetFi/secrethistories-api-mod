namespace SHRestAPI.JsonTranslation
{
    using System;

    /// <summary>
    /// Exception thrown when a translation to or from JSON fails.
    /// </summary>
    public class JsonTranslationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonTranslationException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public JsonTranslationException(string message)
            : base(message)
        {
        }
    }
}
