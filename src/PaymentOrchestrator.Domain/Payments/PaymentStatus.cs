namespace PaymentOrchestrator.Domain.Payments;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Voided,
    Refunded
}
