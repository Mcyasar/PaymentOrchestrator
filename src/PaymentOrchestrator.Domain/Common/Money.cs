namespace PaymentOrchestrator.Domain.Common;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    public Money(decimal amount, string currencyCode)
        : this(amount, new Currency(currencyCode))
    {
    }

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency && Currency.Code != other.Currency.Code)
        {
            throw new InvalidOperationException($"Cannot add money with currency {other.Currency.Code} to {Currency.Code}.");
        }

        return new Money(Amount + other.Amount, Currency);
    }

    public static Money operator +(Money left, Money right)
    {
        ArgumentNullException.ThrowIfNull(left);
        return left.Add(right);
    }
}

