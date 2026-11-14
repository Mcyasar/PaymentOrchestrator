using FluentAssertions;
using PaymentOrchestrator.Domain.Common;
using PaymentOrchestrator.Domain.Payments;
using Xunit;

namespace PaymentOrchestrator.UnitTests.Domain.Payments;

public class PaymentTests
{
    [Fact]
    public void Create_WithValidAmountAndCustomerId_ShouldSetStatusToPendingAndGeneratePaymentId()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        string customerId = "cust_123";

        // When
        var result = Payment.Create(amount, customerId);

        // Then
        result.IsSuccess.Should().BeTrue();
        var payment = result.Value;
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Id.Value.Should().NotBeEmpty();
        payment.Amount.Should().Be(amount);
        payment.CustomerId.Should().Be(customerId);
        payment.TransactionId.Should().BeNull();
    }

    [Fact]
    public void Authorize_WhenPending_ShouldTransitionToAuthorizedAndRecordTransactionId()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        var transactionId = TransactionId.Create("tx_999");

        // When
        var authResult = payment.Authorize(transactionId);

        // Then
        authResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Authorized);
        payment.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void Authorize_WhenAlreadyAuthorized_ShouldReturnFailureResult()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        var transactionId = TransactionId.Create("tx_999");
        payment.Authorize(transactionId);

        // When
        var secondAuthResult = payment.Authorize(TransactionId.Create("tx_1000"));

        // Then
        secondAuthResult.IsFailure.Should().BeTrue();
        secondAuthResult.Error.Code.Should().Be("Payment.InvalidStatusTransition");
    }

    [Fact]
    public void Fail_WhenPending_ShouldTransitionToFailedStatus()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;

        // When
        var failResult = payment.Fail("Insufficient funds");

        // Then
        failResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Failed);
    }
}
