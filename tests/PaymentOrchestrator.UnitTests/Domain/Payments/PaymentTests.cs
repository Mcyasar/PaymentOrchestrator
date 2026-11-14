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
    public void Capture_WhenAuthorized_ShouldTransitionToCapturedStatus()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        payment.Authorize(TransactionId.Create("tx_999"));

        // When
        var captureResult = payment.Capture();

        // Then
        captureResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public void Capture_DirectFromPending_WithTransactionId_ShouldTransitionToCapturedStatus()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        var transactionId = TransactionId.Create("tx_999");

        // When
        var captureResult = payment.Capture(transactionId);

        // Then
        captureResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Captured);
        payment.TransactionId.Should().Be(transactionId);
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

    [Fact]
    public void Capture_WhenFailed_ShouldReturnFailureResultForInvalidStatusTransition()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        payment.Fail("Card declined");

        // When
        var captureResult = payment.Capture();

        // Then
        captureResult.IsFailure.Should().BeTrue();
        captureResult.Error.Code.Should().Be("Payment.InvalidStatusTransition");
    }

    [Fact]
    public void Void_WhenAuthorized_ShouldTransitionToVoidedStatus()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        payment.Authorize(TransactionId.Create("tx_999"));

        // When
        var voidResult = payment.Void("User cancelled");

        // Then
        voidResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Voided);
    }

    [Fact]
    public void Refund_WhenCaptured_ShouldTransitionToRefundedStatus()
    {
        // Given
        var amount = new Money(100.00m, "USD");
        var paymentResult = Payment.Create(amount, "cust_123");
        var payment = paymentResult.Value;
        payment.Authorize(TransactionId.Create("tx_999"));
        payment.Capture();

        // When
        var refundResult = payment.Refund("Item returned");

        // Then
        refundResult.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }
}
