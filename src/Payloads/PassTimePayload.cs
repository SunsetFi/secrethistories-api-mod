namespace SHRestAPI.Payloads
{
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Represents a payload for passing time.
    /// </summary>
    public class PassTimePayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PassTimePayload"/> class.
        /// </summary>
        public PassTimePayload()
        {
        }

        /// <summary>
        /// Gets or sets the seconds to pass.
        /// </summary>
        /// <value>The seconds to pass.</value>
        public float Seconds { get; set; }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        /// <exception cref="BadRequestException">The payload is invalid.</exception>
        public void Validate()
        {
            if (this.Seconds <= 0)
            {
                throw new BadRequestException("Seconds must be greater than 0.");
            }
        }
    }
}
