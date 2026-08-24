using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Catalog;

public sealed class Inventory : Entity
{
    private Inventory() { }

    public Inventory(Guid productVariantId, int availableQuantity)
    {
        if (productVariantId == Guid.Empty) throw new ArgumentException("Product variant ID is required.", nameof(productVariantId));
        if (availableQuantity < 0) throw new ArgumentOutOfRangeException(nameof(availableQuantity));
        ProductVariantId = productVariantId;
        AvailableQuantity = availableQuantity;
    }

    public Guid ProductVariantId { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }

    public int GetSellableQuantity() => AvailableQuantity - ReservedQuantity;

    public void Reserve(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (quantity > GetSellableQuantity()) throw new InvalidOperationException("Insufficient inventory.");
        ReservedQuantity += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ReleaseReservation(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity) throw new ArgumentOutOfRangeException(nameof(quantity));
        ReservedQuantity -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void FinalizeReservation(int quantity)
    {
        if (quantity <= 0 || quantity > ReservedQuantity) throw new ArgumentOutOfRangeException(nameof(quantity));
        ReservedQuantity -= quantity;
        AvailableQuantity -= quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        AvailableQuantity += quantity;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
