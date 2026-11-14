namespace PaymentOrchestrator.Domain.Payments;

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId NewId() => throw new NotImplementedException();
}
