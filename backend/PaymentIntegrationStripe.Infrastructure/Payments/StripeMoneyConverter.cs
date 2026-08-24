using PaymentIntegrationStripe.Application.Payments;

namespace PaymentIntegrationStripe.Infrastructure.Payments;

public sealed class StripeMoneyConverter : IMoneyConverter
{
    private static readonly HashSet<string> ZeroDecimalCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "bif", "clp", "djf", "gnf", "jpy", "kmf", "krw", "mga", "pyg", "rwf", "ugx", "vnd", "vuv", "xaf", "xof", "xpf"
    };

    public long ToMinorUnits(decimal amount, string currency)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

        var exponent = ZeroDecimalCurrencies.Contains(currency) ? 0 : 2;
        var factor = exponent == 0 ? 1m : 100m;
        var minor = decimal.Round(amount * factor, 0, MidpointRounding.ToEven);

        if (minor > long.MaxValue) throw new OverflowException("Amount exceeds provider integer limits.");
        return checked((long)minor);
    }
}
