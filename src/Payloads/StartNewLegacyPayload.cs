namespace SHRestAPI.Payloads
{
    using SecretHistories.Entities;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// A payload for starting a new legacy.
    /// </summary>
    public class StartNewLegacyPayload
    {
        /// <summary>
        /// Gets or sets the legacy id.
        /// </summary>
        public string LegacyId { get; set; }

        /// <summary>
        /// Gets the legacy indicated by this payload.
        /// </summary>
        public Legacy Legacy
        {
            get
            {
                return Watchman.Get<Compendium>().GetEntityById<Legacy>(this.LegacyId);
            }
        }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        /// <exception cref="BadRequestException">The payload is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.LegacyId))
            {
                throw new BadRequestException("Legacy ID must be specified.");
            }

            var legacy = Watchman.Get<Compendium>().GetEntityById<Legacy>(this.LegacyId);
            if (!legacy.IsValid())
            {
                throw new BadRequestException($"Legacy {this.LegacyId} does not exist.");
            }
        }
    }
}
