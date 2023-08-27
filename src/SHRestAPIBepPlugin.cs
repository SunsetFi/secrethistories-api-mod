namespace SHRestAPI
{
    using System;
    using BepInEx;
    using BepInEx.Logging;

    /// <summary>
    /// The bepinex plugin for SHRestAPI.  Delegates to the CS style plugin.
    /// </summary>
    [BepInPlugin("com.robophred.secrethistories.shrestapi", "Secret Histories RESTAPI", "1.0.0.0")]
    public class SHRestAPIBepPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// Gets the instance of the plugin.
        /// </summary>
        public static ManualLogSource PublicLogger { get; private set; }

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        private void Awake()
        {
            this.Logger.LogInfo("Secret Histories RESTAPI plugin loaded.");
            PublicLogger = this.Logger;
            try
            {
                SHRest.Initialise();
                this.Logger.LogInfo("SH.Initialize completed.");
            }
            catch (Exception ex)
            {
                this.Logger.LogError("Error initialising SHRestAPI: " + ex.ToString());
            }
        }

        private void OnDestroy()
        {
            Logging.LogTrace($"SHRestAPI bep plugin destroyed.");
        }

        private void Update()
        {
            Dispatcher.Drain();
        }
    }
}
