using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() { }

    internal OrderItem(Guid orderId, Guid productVariantId, string sku, string productName, decimal unitPrice, int quantity)
    {
        OrderId = orderId;
        ProductVariantId = productVariantId;
        Sku = sku;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }
    public Guid ProductVariantId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string ProductName { get; private set; } = null!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.ToEven);
}
