#if BH
namespace SHRestAPI
{
    using BepInEx;
    using BepInEx.Logging;
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using UnityEngine.Events;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// The bepinex plugin for SHRestAPI.  Delegates to the CS style plugin.
    /// </summary>
    [BepInPlugin("com.robophred.secrethistories.shrestapi", "Secret Histories RESTAPI", "1.0.0.0")]
    public class SHRestAPIBepPlugin : BaseUnityPlugin
    {
        public static ManualLogSource PublicLogger { get; private set; }

        /// <summary>
        /// Called when the plugin is loaded.
        /// </summary>
        private void Awake()
        {
            this.Logger.LogInfo("Secret Histories RESTAPI BepnEx plugin loaded.");
            PublicLogger = this.Logger;

            SceneManager.sceneUnloaded += new UnityAction<Scene>(this.HandleSceneUnloaded);

            // Note: this is a hack to get the SHRestAPI to load.  It's not a great solution, but it works.
            // SHRest will be destroyed after this.
            SHRestServer.Initialize();
        }

        private void OnDestroy()
        {
            Logging.LogTrace($"SHRestAPI bep plugin destroyed.");
        }

        private void Update()
        {
            Dispatcher.Drain();
            GameEventSource.RaiseGameTick();
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            // There's a null crash somewhere in here with GOG.  Maybe the compendium isn't initialized when the first scene unloads?
            // Perhaps there's some sort of other splash screen?>
            var compendium = Watchman.Get<Compendium>();
            if (compendium == null)
            {
                return;
            }

            var dictum = compendium.GetSingleEntity<Dictum>();
            if (dictum == null)
            {
                // Game hasn't loaded yet, so obviously this cannot be the game ending.
                return;
            }

            if (scene.name == dictum.PlayfieldScene)
            {
                // FIXME: This wont happen if we load a game while running another game.
                GameEventSource.RaiseGameEnded();
            }
        }
    }
}
#endif
