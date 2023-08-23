namespace SHRestAPI.Payloads
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Commands.Encausting;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Infrastructure.Persistence;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// A payload for a request to hydrate save data.
    /// </summary>
    public class HydrateGameStatePayload
    {
        /// <summary>
        /// Gets or sets the game state to hydrate.
        /// </summary>
        public JObject GameState { get; set; }

        /// <summary>
        /// Gets the game state provider for the provided save data.
        /// </summary>
        public GamePersistenceProvider Provider
        {
            get
            {
                return new HydratedPersistenceProvider(this.GameState.ToString());
            }
        }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        /// <exception cref="BadRequestException">The payload contains invalid data.</exception>
        public void Validate()
        {
            if (this.GameState == null)
            {
                throw new BadRequestException("No game state provided.");
            }
        }

        private class HydratedPersistenceProvider : GamePersistenceProvider
        {
            private readonly string content;

            public HydratedPersistenceProvider(string content)
            {
                this.content = content;
            }

            public override GameSpeed GetDefaultGameSpeed()
            {
                return GameSpeed.Paused;
            }

            public override async Task<bool> SerialiseAndSaveAsyncWithDefaultSaveName()
            {
                // Save as a 'normal' game.
                return await this.SerialiseAndSaveAsync("save.json");
            }

            public override void DepersistGameState()
            {
                this._saveValidity = SaveValidity.OK;
                this._persistedGameState = new SerializationHelper().DeserializeFromJsonString<PersistedGameState>(this.content);
            }

            protected override string GetSaveFileLocation()
            {
                return this.GetSaveFileLocation("save.json");
            }

            protected override string GetSaveFileLocation(string saveName)
            {
                return Watchman.Get<MetaInfo>().PersistentDataPath + "/" + saveName;
            }
        }
    }
}
