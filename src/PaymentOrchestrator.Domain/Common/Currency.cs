namespace PaymentOrchestrator.Domain.Common;

public sealed record Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Currency code cannot be null or empty.", nameof(code));
        }

        Code = code;
    }

    public static Currency Create(string code) => new(code);
}

