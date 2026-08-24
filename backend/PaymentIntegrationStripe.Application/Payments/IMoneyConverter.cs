namespace PaymentIntegrationStripe.Application.Payments;

public interface IMoneyConverter
{
    long ToMinorUnits(decimal amount, string currency);
}
