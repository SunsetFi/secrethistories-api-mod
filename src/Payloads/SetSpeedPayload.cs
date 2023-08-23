namespace SHRestAPI.Payloads
{
    using System;
    using SecretHistories.Enums;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Represents a payload for setting the game speed.
    /// </summary>
    public class SetSpeedPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetSpeedPayload"/> class.
        /// </summary>
        public SetSpeedPayload()
        {
        }

        /// <summary>
        /// Gets or sets the speed.
        /// </summary>
        /// <value>The speed of the game.</value>
        public string Speed { get; set; }

        /// <summary>
        /// Gets the game speed from the payload speed.
        /// </summary>
        public GameSpeed GameSpeed
        {
            get
            {
                return Enum.Parse<GameSpeed>(this.Speed);
            }
        }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        /// <exception cref="BadRequestException">The payload is invalid.</exception>
        public void Validate()
        {
            if (!Enum.TryParse(this.Speed, out GameSpeed _))
            {
                throw new BadRequestException("Invalid speed.");
            }
        }
    }
}
