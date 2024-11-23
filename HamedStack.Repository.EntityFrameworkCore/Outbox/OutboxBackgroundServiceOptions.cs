namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox
{
    /// <summary>
    /// Options for configuring the behavior of the <see cref="OutboxBackgroundService"/>.
    /// </summary>
    public class OutboxBackgroundServiceOptions
    {
        /// <summary>
        /// Gets or sets the polling interval in seconds for the background service.
        /// Default is 10 seconds.
        /// </summary>
        public int PollingIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum number of messages to process in a single batch.
        /// Default is 100.
        /// </summary>
        public int BatchSize { get; set; } = 100;
    }
}
