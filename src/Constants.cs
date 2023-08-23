namespace SHRestAPI
{
    /// <summary>
    /// Shared constants.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// Gets the name of the game scene.
        /// </summary>
#if BH
        public static string GameScene => "S4Library";
#else
        public static string GameScene => "S4Tabletop";
#endif
    }
}
