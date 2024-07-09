namespace SHRestAPI
{
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using SecretHistories.Entities;
    using SecretHistories.Infrastructure;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;
    using SHRestAPI.Tasks;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Responsible for waiting for the game to settle down after performing events.
    /// </summary>
    public static class Settler
    {
        /// <summary>
        /// Waits for the game to be loaded.
        /// </summary>
        /// <returns>A task that resolves when the game has fully loaded.</returns>
        /// <remarks>
        /// This is hugely messy, but there's no real way to track the active state of the game across scene transitions.
        /// Detecting loading new games when a game is already ongoing is particularly tricky.
        /// </remarks>
        public static async Task AwaitGameReady()
        {
            // Note: This may be null if we are called too soon.
            // This should never happen, as we really shouldn't be requesting loads before the game has initialized its data.
            // TODO: We may want to make this guarentee more robust, or actually await the load.
            // However, if this is null, we will definitely not be in the process of loading a save, unless
            // another request on another thread triggers it.
            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
            if (dictum == null)
            {
                throw new ConflictException("Game has not initialized.");
            }

            // First, await the game scene.
            await AwaitConditionTask.From(GameSceneIsLoaded, CancellationToken.None);

            // Then, wait for the load process to start.
            // We can peek into this by waiting until we have a character.
            var stable = Watchman.Get<Stable>();
            await AwaitConditionTask.From(() => stable.Protag() != null, CancellationToken.None);

            // Then, wait for the game to fully load, as indicated by the player having control.
            var nexus = Watchman.Get<LocalNexus>();
            await AwaitConditionTask.From(() => nexus.PlayerInputDisabled() == false || !GameSceneIsLoaded(), CancellationToken.None);

            if (!GameSceneIsLoaded())
            {
                // This might be due to a corrupted save.
                throw new InternalServerErrorException("Game failed to load.");
            }
        }

        /// <summary>
        /// Waits for the game to settle.
        /// </summary>
        /// <returns>A task that resolves when the game has no ongoing tasks that are incomplete.</returns>
        /// <remarks>
        /// Settling is defined as having no ongoing token itineraries.
        /// </remarks>
        public static async Task AwaitSettled()
        {
            while (await IsSettled() == false)
            {
                await Task.Delay(100);
            }

            // Would be great to do this, but it looks like this wont stop the animations, and the animation will also call Arrive on completed.
            // var itineraries = Watchman.Get<Xamanek>().CurrentItineraries.Values.ToArray();
            // foreach (var itinerary in itineraries)
            // {
            //     itinerary.Arrive(itinerary.GetToken(), new Context(Context.ActionSource.Debug));
            // }
        }

        private static ConfiguredTaskAwaitable<bool> IsSettled()
        {
            // This is kinda hackish.  Autoccultist has a more involved but significantly better way of handling threading.
            return Dispatcher.DispatchRead(() =>
            {
                var xamanek = Watchman.Get<Xamanek>();
                if (xamanek == null)
                {
                    // Game is not active, that's settled enough.
                    return true;
                }

                return xamanek.CurrentItineraries.Count == 0;
            });
        }

        private static bool GameSceneIsLoaded()
        {
            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
            var gameScene = SceneManager.GetSceneByName(dictum.PlayfieldScene);
            return gameScene.isLoaded;
        }
    }
}
