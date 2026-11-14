namespace PaymentOrchestrator.Domain.Payments;

public readonly record struct TransactionId(string Value)
{
    public static TransactionId Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(value));
        }

        return new TransactionId(value);
    }
}

