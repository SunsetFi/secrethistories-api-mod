using System;
using SecretHistories.Entities;
using SecretHistories.UI;
using SHRestAPI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// This is the entry point for the SHRestAPI mod.
/// </summary>
public class SHRest : MonoBehaviour
{
    /// <summary>
    /// Initialize the mod.
    /// </summary>
    public static void Initialise()
    {
        Logging.LogInfo("SHRestAPI Initializing.");
        try
        {
            var go = new GameObject();
            go.AddComponent<SHRest>();
            DontDestroyOnLoad(go);
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to initialize SHRestAPI: {ex}");
        }
    }

    /// <summary>
    /// Unity callback for when the game object is created.
    /// This registers our controllers and starts the server.
    /// </summary>
    public void Awake()
    {
        Logging.LogInfo("SHRestAPI Initializing.");
        try
        {
            SceneManager.sceneUnloaded += new UnityAction<Scene>(this.HandleSceneUnloaded);
            SHRestServer.Initialize();
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to start SHRestAPI: {ex}");
            return;
        }
    }

    /// <summary>
    /// Unity callback for when the game updates.
    /// </summary>
    public void Update()
    {
        Dispatcher.Drain();
        GameEventSource.RaiseGameTick();
    }

    private void OnDestroy()
    {
        Logging.LogTrace($"SHRestAPI destroyed.");
    }

    private void HandleSceneUnloaded(Scene scene)
    {
        var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
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
