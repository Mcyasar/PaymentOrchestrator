using FluentAssertions;
using PaymentOrchestrator.Domain.Common;
using Xunit;

namespace PaymentOrchestrator.UnitTests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_WithValue_ShouldIndicateSuccessAndProvideValue()
    {
        // Given
        var money = new Money(100.00m, "USD");

        // When
        var result = Result.Success(money);

        // Then
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(money);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WithError_ShouldIndicateFailureAndProvideError()
    {
        // Given
        var error = new Error("Payment.Failed", "Payment processing failed");

        // When
        var result = Result.Failure<Money>(error);

        // Then
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_AccessingValueOnFailure_ShouldThrowInvalidOperationException()
    {
        // Given
        var error = new Error("Payment.Failed", "Payment processing failed");
        var result = Result.Failure<Money>(error);

        // When & Then
        Action act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>();
    }
}
