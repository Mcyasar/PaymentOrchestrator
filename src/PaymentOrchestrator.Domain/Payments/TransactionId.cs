namespace PaymentOrchestrator.Domain.Payments;

public readonly record struct TransactionId(string Value)
{
    public static TransactionId Create(string value) => throw new NotImplementedException();
}
