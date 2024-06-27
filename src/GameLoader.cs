namespace SHRestAPI
{
    using System.Threading;
    using System.Threading.Tasks;
    using HarmonyLib;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;
    using SHRestAPI.Tasks;
    using UnityEngine.SceneManagement;

    public static class GameLoader
    {
        public async static Task LoadGameFromSource(GamePersistenceProvider source)
        {
            var stageHand = Watchman.Get<StageHand>();
            var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();

            var awaitReady = await Dispatcher.DispatchWrite(() =>
            {
                source.DepersistGameState();
                if (source.IsSaveCorrupted())
                {
                    throw new UnprocessableEntityException("Save file is corrupted.");
                }

                // TODO: BH explicity hides its game menu.  Do we need to do that?
                // Won't it get unloaded when the scene does?
                if (source.GetCharacterState() == SecretHistories.Enums.CharacterState.Extinct)
                {
                    stageHand.NewGameScreen();
                    return null;
                }
                else
                {
                    // LoadGameInPlayfieldWithLoadingScreen
                    SoundManager.PlaySfxUnrefined(AudioEvent.UIStartGame);
                    stageHand.UsePersistenceProvider(source);

                    // SceneChangeWithLoadingScreen
                    Logging.LogTrace($"We are unloading the formost scene, which is {stageHand.GetForemostScene().name}");
                    var unloadOp = SceneManager.UnloadSceneAsync(stageHand.GetForemostScene());
                    Traverse.Create(stageHand).Method("CleanupNonPersistentFucineObjects").GetValue();
                    Traverse.Create(stageHand).Field("_queuedSceneToLoad").SetValue(dictum.PlayfieldScene);
                    var loadOp = SceneManager.LoadSceneAsync(dictum.LoadingScene, LoadSceneMode.Additive);
                    return AwaitConditionTask.From(() => unloadOp.isDone && loadOp.isDone, CancellationToken.None);
                }
            });

            if (awaitReady != null)
            {
                await awaitReady;
                Logging.LogTrace($"Apparently the async loading is done.  Is it? loading: {SceneManager.GetSceneByName(dictum.LoadingScene).isLoaded} game: {SceneManager.GetSceneByName(dictum.PlayfieldScene).isLoaded}");
                await Settler.AwaitGameReady();
                await Settler.AwaitSettled();
            }
        }
    }
}