using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments;

public sealed class Payment
{
    public PaymentId Id { get; }
    public Money Amount { get; }
    public string CustomerId { get; }
    public PaymentStatus Status { get; }
    public TransactionId? TransactionId { get; }

    private Payment(PaymentId id, Money amount, string customerId)
    {
        throw new NotImplementedException();
    }

    public static Result<Payment> Create(Money amount, string customerId)
    {
        throw new NotImplementedException();
    }

    public Result Authorize(TransactionId transactionId)
    {
        throw new NotImplementedException();
    }

    public Result Fail(string reason)
    {
        throw new NotImplementedException();
    }
}
