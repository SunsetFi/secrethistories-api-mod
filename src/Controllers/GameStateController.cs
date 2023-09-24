namespace SHRestAPI.Controllers
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Abstract;
    using SecretHistories.Commands.Encausting;
    using SecretHistories.Entities;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;

    /// <summary>
    /// Controller for interacting with the game state as a whole.
    /// </summary>
    [WebController(Path = "api/game-state")]
    public class GameStateController
    {
        /// <summary>
        /// Gets a save file representing the current game state.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "GET")]
        public async Task GetGameState(IHttpContext context)
        {
            var gameState = await Dispatcher.DispatchRead(() =>
            {
                var persistenceProvider = new DefaultGamePersistenceProvider();
                persistenceProvider.Encaust(Watchman.Get<Stable>(), FucineRoot.Get(), Watchman.Get<Xamanek>());
                var persistedGameState = (IEncaustment)typeof(GamePersistenceProvider).GetField("_persistedGameState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(persistenceProvider);
                string json = new SerializationHelper().SerializeToJsonString(persistedGameState);
                return JObject.Parse(json);
            });

            await context.SendResponse(HttpStatusCode.OK, new
            {
                gameState,
            });
        }

        /// <summary>
        /// Hydrates the game state in a new game.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "PUT")]
        public async Task HydrateGameState(IHttpContext context)
        {
            var payload = context.ParseBody<HydrateGameStatePayload>();
            payload.Validate();

            await Dispatcher.DispatchWrite(() =>
            {
                var stageHand = Watchman.Get<StageHand>();

                // Switch scenes without using LoadGameOnTabletop.
                // Bit janky, but we would need to await the fades, and LoadGameOnTabletop doesn't return a task.
                stageHand.UsePersistenceProvider(payload.Provider);

                // Roslyn says 'new object[]' can be simplified to '[]'.  Don't believe its lies.  Apparently our csproj doesn't support that.
                typeof(StageHand).GetMethod("SceneChange", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stageHand, new object[] { Watchman.Get<Compendium>().GetSingleEntity<Dictum>().PlayfieldScene, false });
            });

            await Settler.AwaitGameReady();

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets the active legacy, if any.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "GET", Path = "legacy")]
        public async Task GetLegacy(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var stageHand = Watchman.Get<StageHand>();
                if (stageHand.SceneIsActive(Constants.GameScene))
                {
                    var protag = Watchman.Get<Stable>().Protag();
                    return new
                    {
                        legacyId = protag.ActiveLegacy.Id,
                        legacyLabel = protag.ActiveLegacy.Label,
                    };
                }

                return null;
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Starts a new legacy.
        /// </summary>
        /// <param name="context">The HTTP request context.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "PUT", Path = "legacy")]
        public async Task StartNewLegacy(IHttpContext context)
        {
            var payload = context.ParseBody<StartNewLegacyPayload>();
            payload.Validate();

            await Dispatcher.DispatchWrite(() =>
            {
                var provider = new FreshGameProvider(payload.Legacy);
                var stageHand = Watchman.Get<StageHand>();

                // Switch scenes without using LoadGameOnTabletop.
                // Bit janky, but we would need to await the fades, and LoadGameOnTabletop doesn't return a task.
                stageHand.UsePersistenceProvider(provider);

                // Roslyn says 'new object[]' can be simplified to '[]'.  Don't believe its lies.  Apparently our csproj doesn't support that.
                typeof(StageHand).GetMethod("SceneChange", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(stageHand, new object[] { Watchman.Get<Compendium>().GetSingleEntity<Dictum>().PlayfieldScene, false });
            });

            await Settler.AwaitGameReady();
            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK);
        }
    }
}
