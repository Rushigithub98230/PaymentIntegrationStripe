using PaymentIntegrationStripe.Infrastructure.Payments;

namespace PaymentIntegrationStripe.Tests;

public class StripeMoneyConverterTests
{
    private readonly StripeMoneyConverter _converter = new();

    [Fact]
    public void Converts_two_decimal_currency_to_minor_units()
    {
        Assert.Equal(12345, _converter.ToMinorUnits(123.45m, "usd"));
    }

    [Fact]
    public void Does_not_multiply_zero_decimal_currency_by_100()
    {
        Assert.Equal(1000, _converter.ToMinorUnits(1000m, "jpy"));
    }
}
