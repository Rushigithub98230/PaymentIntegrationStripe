using PaymentIntegrationStripe.Domain.Common;

namespace PaymentIntegrationStripe.Domain.Orders;

public sealed class Order : Entity
{
    private Order() { }

    public Order(Guid customerId, decimal subtotal, decimal discount, decimal tax, decimal shipping, decimal total, string currency)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));
        if (subtotal < 0 || discount < 0 || tax < 0 || shipping < 0 || total <= 0) throw new ArgumentOutOfRangeException(nameof(total));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

        CustomerId = customerId;
        Subtotal = decimal.Round(subtotal, 2, MidpointRounding.ToEven);
        Discount = decimal.Round(discount, 2, MidpointRounding.ToEven);
        Tax = decimal.Round(tax, 2, MidpointRounding.ToEven);
        Shipping = decimal.Round(shipping, 2, MidpointRounding.ToEven);
        Total = decimal.Round(total, 2, MidpointRounding.ToEven);
        Currency = currency.Trim().ToLowerInvariant();
        Status = OrderStatus.PendingPayment;
    }

    public Guid CustomerId { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Shipping { get; private set; }
    public decimal Total { get; private set; }
    public string Currency { get; private set; } = null!;
    public OrderStatus Status { get; private set; }
    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    public void AddItem(Guid productVariantId, string sku, string productName, decimal unitPrice, int quantity)
    {
        if (productVariantId == Guid.Empty) throw new ArgumentException("Product variant ID is required.", nameof(productVariantId));
        if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(productName)) throw new ArgumentException("Product snapshot is required.");
        if (unitPrice < 0 || quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        Items.Add(new OrderItem(Id, productVariantId, sku.Trim(), productName.Trim(), decimal.Round(unitPrice, 2, MidpointRounding.ToEven), quantity));
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void TransitionTo(OrderStatus next)
    {
        if (!OrderStateMachine.CanTransition(Status, next))
            throw new InvalidOperationException($"Invalid order transition: {Status} -> {next}.");
        Status = next;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
