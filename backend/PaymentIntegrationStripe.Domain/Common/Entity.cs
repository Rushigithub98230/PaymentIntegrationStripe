namespace PaymentIntegrationStripe.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    // SQL Server maintains this value and EF Core uses it for optimistic concurrency.
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
}
