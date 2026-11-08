namespace PaymentOrchestrator.Domain.Common;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        throw new NotImplementedException();
    }

    public Money(decimal amount, string currencyCode)
    {
        throw new NotImplementedException();
    }

    public Money Add(Money other)
    {
        throw new NotImplementedException();
    }

    public static Money operator +(Money left, Money right)
    {
        throw new NotImplementedException();
    }
}
