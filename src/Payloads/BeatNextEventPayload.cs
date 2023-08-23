namespace SHRestAPI.Payloads
{
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Represents a payload for setting the next event to be beaten.
    /// </summary>
    public class BeatNextEventPayload
    {
        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        /// <value>The name of the event.</value>
        public string Event { get; set; }

        /// <summary>
        /// Validates the payload contents.
        /// </summary>
        /// <exception cref="BadRequestException">Thrown when the Event is not valid.</exception>
        public void Validate()
        {
            if (this.Event != "CardDecay" && this.Event != "RecipeCompletion" && this.Event != "Either")
            {
                throw new BadRequestException($"Invalid event: {this.Event}. Expected 'Either', 'CardDecay' or 'RecipeCompletion'.");
            }
        }
    }
}
