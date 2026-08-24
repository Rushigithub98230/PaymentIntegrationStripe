using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Catalog;

public sealed class ProductVariant : Entity
{
    private ProductVariant() { }

    public ProductVariant(Guid productId, string sku, decimal price, string currency)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Product ID is required.", nameof(productId));
        if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("SKU is required.", nameof(sku));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

        ProductId = productId;
        Sku = sku.Trim();
        Price = decimal.Round(price, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToLowerInvariant();
        IsActive = true;
    }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = null!;
    public bool IsActive { get; private set; }

    public void ChangePrice(decimal price, string currency)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        Price = decimal.Round(price, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToLowerInvariant();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
