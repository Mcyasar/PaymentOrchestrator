namespace PaymentOrchestrator.Domain.Common;

public sealed record Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        throw new NotImplementedException();
    }

    public static Currency Create(string code)
    {
        throw new NotImplementedException();
    }
}
