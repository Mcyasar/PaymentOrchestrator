namespace PaymentOrchestrator.Domain.Payments;

public readonly record struct PaymentId(Guid Value)
{
    public static PaymentId NewId() => new(Guid.NewGuid());
}

