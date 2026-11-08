namespace PaymentOrchestrator.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        throw new NotImplementedException();
    }

    public static Result Success()
    {
        throw new NotImplementedException();
    }

    public static Result Failure(Error error)
    {
        throw new NotImplementedException();
    }

    public static Result<TValue> Success<TValue>(TValue value)
    {
        throw new NotImplementedException();
    }

    public static Result<TValue> Failure<TValue>(Error error)
    {
        throw new NotImplementedException();
    }
}

public class Result<TValue> : Result
{
    public TValue Value => throw new NotImplementedException();

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        throw new NotImplementedException();
    }

    public static implicit operator Result<TValue>(TValue? value)
    {
        throw new NotImplementedException();
    }
}
