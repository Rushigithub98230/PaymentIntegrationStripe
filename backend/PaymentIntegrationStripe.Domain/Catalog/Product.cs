using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Catalog;

public sealed class Product : Entity
{
    private Product() { }

    public Product(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required.", nameof(name));
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        IsActive = true;
    }

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public ICollection<ProductVariant> Variants { get; private set; } = new List<ProductVariant>();

    public void SetActive(bool active)
    {
        IsActive = active;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
