using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Domain.Payments;

public sealed class Payment
{
    private static readonly Error InvalidStatusTransition = new("Payment.InvalidStatusTransition", "Invalid payment status transition.");

    public PaymentId Id { get; private set; }
    public Money Amount { get; private set; }
    public string CustomerId { get; private set; }
    public PaymentStatus Status { get; private set; }
    public TransactionId? TransactionId { get; private set; }

    private Payment(PaymentId id, Money amount, string customerId)
    {
        Id = id;
        Amount = amount ?? throw new ArgumentNullException(nameof(amount));
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        Status = PaymentStatus.Pending;
    }

    public static Result<Payment> Create(Money amount, string customerId)
    {
        if (amount is null)
        {
            return Result.Failure<Payment>(new Error("Payment.NullAmount", "Amount cannot be null."));
        }

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return Result.Failure<Payment>(new Error("Payment.NullCustomerId", "Customer ID cannot be empty."));
        }

        var payment = new Payment(PaymentId.NewId(), amount, customerId);
        return Result.Success(payment);
    }

    public Result Authorize(TransactionId transactionId)
    {
        if (Status != PaymentStatus.Pending)
        {
            return Result.Failure(InvalidStatusTransition);
        }

        Status = PaymentStatus.Authorized;
        TransactionId = transactionId;
        return Result.Success();
    }

    public Result Capture(TransactionId? transactionId = null)
    {
        if (Status == PaymentStatus.Authorized)
        {
            if (transactionId.HasValue)
            {
                TransactionId = transactionId.Value;
            }

            Status = PaymentStatus.Captured;
            return Result.Success();
        }

        if (Status == PaymentStatus.Pending && transactionId.HasValue)
        {
            TransactionId = transactionId.Value;
            Status = PaymentStatus.Captured;
            return Result.Success();
        }

        return Result.Failure(InvalidStatusTransition);
    }

    public Result Fail(string reason)
    {
        if (Status != PaymentStatus.Pending && Status != PaymentStatus.Authorized)
        {
            return Result.Failure(InvalidStatusTransition);
        }

        Status = PaymentStatus.Failed;
        return Result.Success();
    }

    public Result Void(string reason)
    {
        if (Status != PaymentStatus.Authorized && Status != PaymentStatus.Pending)
        {
            return Result.Failure(InvalidStatusTransition);
        }

        Status = PaymentStatus.Voided;
        return Result.Success();
    }

    public Result Refund(string reason)
    {
        if (Status != PaymentStatus.Captured)
        {
            return Result.Failure(InvalidStatusTransition);
        }

        Status = PaymentStatus.Refunded;
        return Result.Success();
    }
}

