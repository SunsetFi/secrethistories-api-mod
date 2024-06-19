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

        [WebRouteMethod(Method = "POST")]
        public async Task LoadGameSave(IHttpContext context, LoadSavePayload body)
        {
            body.Validate();

            await Dispatcher.DispatchWrite(() =>
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
                }
                else
                {
                    Watchman.Get<StageHand>().LoadGameInPlayfieldWithLoadingScreen(source, Watchman.Get<StageHand>().GetForemostScene());
                }
            });

            await Settler.AwaitGameReady();

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.NoContent);
        }
    }
#endif
}
