namespace SHRestAPI.Controllers
{
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Abstract;
    using SecretHistories.Commands.Encausting;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

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
        /// <param name="body">The payload for hydrating the game state.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "PUT")]
        public async Task HydrateGameState(IHttpContext context, HydrateGameStatePayload body)
        {
            body.Validate();

            await GameLoader.LoadGameFromSource(body.Provider);

            await context.SendResponse(HttpStatusCode.NoContent);
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
                var dictum = Watchman.Get<Compendium>().GetSingleEntity<Dictum>();
                if (stageHand.SceneIsActive(dictum.PlayfieldScene))
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
        /// <param name="body">The payload for starting a new legacy.</param>
        /// <returns>A task that completes when the request is handled.</returns>
        [WebRouteMethod(Method = "PUT", Path = "legacy")]
        public async Task StartNewLegacy(IHttpContext context, StartNewLegacyPayload body)
        {
            body.Validate();

            var provider = new FreshPausedGameProvider(body.Legacy);
            await GameLoader.LoadGameFromSource(provider);

            await context.SendResponse(HttpStatusCode.NoContent);
        }

        private class FreshPausedGameProvider : FreshGameProvider
        {
            public FreshPausedGameProvider(Legacy legacy)
                : base(legacy)
            {
            }

            public override GameSpeed GetDefaultGameSpeed()
            {
                return GameSpeed.Paused;
            }
        }
    }
}
