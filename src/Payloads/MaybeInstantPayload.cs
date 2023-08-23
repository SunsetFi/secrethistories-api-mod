namespace SHRestAPI.Payloads
{
    /// <summary>
    /// A payload optionally indicating if an operation is instant.
    /// </summary>
    public class MaybeInstantPayload
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation is instant.
        /// </summary>
        public bool Instant { get; set; }
    }
}
