namespace SHRestAPI
{
    /// <summary>
    /// Extension methods for the <see cref="float"/> type.
    /// </summary>
    public static class FloatExtensions
    {
        /// <summary>
        /// Converts a float to a nullable float, returning null if the float is NaN.
        /// </summary>
        /// <param name="value">The float value.</param>
        /// <returns>The value.</returns>
        public static float? NanToNull(this float value)
        {
            if (float.IsNaN(value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Converts a float to a nullable float, returning a default value if the float is NaN.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The value with default applied.</returns>
        public static float NanToDefault(this float value, float defaultValue)
        {
            if (float.IsNaN(value))
            {
                return defaultValue;
            }

            return value;
        }
    }
}
