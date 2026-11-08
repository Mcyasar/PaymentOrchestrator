using FluentAssertions;
using PaymentOrchestrator.Domain.Common;
using Xunit;

namespace PaymentOrchestrator.UnitTests.Domain.Common;

public class MoneyAndCurrencyTests
{
    [Fact]
    public void Create_WithAmountAndCurrency_ShouldRetainAmountAndCurrency()
    {
        // Given
        string currencyCode = "USD";
        decimal amount = 100.50m;

        // When
        var money = new Money(amount, currencyCode);

        // Then
        money.Amount.Should().Be(amount);
        money.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        // Given
        var money1 = new Money(100.50m, "USD");
        var money2 = new Money(49.50m, "USD");

        // When
        var result = money1.Add(money2);

        // Then
        result.Amount.Should().Be(150.00m);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrencies_ShouldThrowInvalidOperationException()
    {
        // Given
        var usd = new Money(100.00m, "USD");
        var eur = new Money(50.00m, "EUR");

        // When & Then
        Action act = () => usd.Add(eur);

        act.Should().Throw<InvalidOperationException>();
    }
}
