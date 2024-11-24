namespace HamedStack.TheRepository.EntityFrameworkCore.Outbox;

/// <summary>
/// Represents a message that is stored in an outbox for further processing.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for the message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the message.
    /// This is used to identify the type or category of the message.
    /// </summary>
    /// <example>
    /// Examples of names can be "OrderCreated" or "UserRegistered".
    /// </example>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the content of the message.
    /// This typically contains the serialized message data in JSON or another format.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    /// <remarks>
    /// This indicates when the message was initially added to the outbox.
    /// </remarks>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the message was processed.
    /// </summary>
    /// <remarks>
    /// If the message has not yet been processed, this value will be <c>null</c>.
    /// </remarks>
    public DateTime? ProcessedOn { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message has been processed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the message has been processed; otherwise, <c>false</c>.
    /// </value>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of times the message has been retried.
    /// </summary>
    /// <remarks>
    /// If the message has not been retried, this value will be <c>null</c> or zero.
    /// This property is useful for tracking and handling transient errors.
    /// </remarks>
    public int? RetryCount { get; set; }
}
