namespace SHRestAPI.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.Services;
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

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

        // TODO: POST to api/saves should create a new save.

        [WebRouteMethod(Method = "POST", Path = "current-save")]
        public async Task LoadGameSave(IHttpContext context, LoadSavePayload body)
        {
            body.Validate();

            var awaitReady = await Dispatcher.DispatchWrite(() =>
            {
                // Recreation of SaveLoadPanel.LoadSelectedSave
                var source = new NamedGamePersistenceProvider(body.SaveName);
                source.DepersistGameState();
                if (source.IsSaveCorrupted())
                {
                    throw new UnprocessableEntityException("Save file is corrupted.");
                }

                // TODO: BH explicity hides its game menu.  Do we need to do that?
                // Won't it get unloaded when the scene does?
                if (source.GetCharacterState() == SecretHistories.Enums.CharacterState.Extinct)
                {
                    Watchman.Get<StageHand>().NewGameScreen();
                    return false;
                }
                else
                {
                    Watchman.Get<StageHand>().LoadGameInPlayfieldWithLoadingScreen(source, Watchman.Get<StageHand>().GetForemostScene());
                    return true;
                }
            });

            if (awaitReady)
            {
                await Settler.AwaitGameReady();
                await Settler.AwaitSettled();
            }

            await context.SendResponse(HttpStatusCode.NoContent);
        }
    }
#endif
}
