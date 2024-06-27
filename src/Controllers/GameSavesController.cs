namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Infrastructure;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

    // TODO: These might work in CS too, as it does have a save system.  It's just not exposed to the user.
#if BH

    /// <summary>
    /// Controller for listing and loading game saves.
    /// </summary>
    [WebController(Path = "api/saves")]
    public class GameSavesController
    {
        [WebRouteMethod(Method = "GET")]
        public async Task GetGameSaves(IHttpContext context)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                var persistence = new DefaultGamePersistenceProvider();
                var saves = persistence.GetSaveFileNames();
                return saves.Select(x => SaveInfoPayload.FromInfo(x)).ToList();
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        [WebRouteMethod(Method = "POST")]
        public async Task CreateGameSave(IHttpContext context, CreateSavePayload body)
        {
            await Dispatcher.DispatchWrite(async () =>
            {
                var gameGateway = Watchman.Get<GameGateway>();
                if (!gameGateway.GameInSaveableState())
                {
                    throw new ConflictException("Game is not currently in a saveable state.");
                }

                bool result;
                if (string.IsNullOrEmpty(body.SaveName))
                {
                    result = await gameGateway.TryDefaultSave();
                }
                else
                {
                    result = await gameGateway.TryNamedSave(body.SaveName);
                }

                if (!result)
                {
                    throw new InternalServerErrorException("Failed to save game.");
                }
            });

            await context.SendResponse(HttpStatusCode.NoContent);
        }

        // TODO: POST to api/saves should create a new save.

        [WebRouteMethod(Method = "POST", Path = "current-save")]
        public async Task LoadGameSave(IHttpContext context, LoadSavePayload body)
        {
            body.Validate();

            var source = new NamedGamePersistenceProvider(body.SaveName);
            await GameLoader.LoadGameFromSource(source);

            await context.SendResponse(HttpStatusCode.NoContent);
        }
    }
#endif
}
