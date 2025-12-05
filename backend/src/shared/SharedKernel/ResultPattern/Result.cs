namespace SharedKernel.ResultPattern;

public class Result
{
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    
    public Error Error { get; }
    
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new
    (
        isSuccess: true,
        error: Error.None
    );
    
    public static Result<TValue> Success<TValue>(TValue value) => new
    (
        value: value,
        isSuccess: true,
        error: Error.None
    );
    
    public static Result Failure(Error error) => new
    (
        isSuccess: false,
        error: error
    );
    
    public static Result<TValue> Failure<TValue>(Error error) => new
    (
        value: default,
        isSuccess: false,
        error: error
    );
    
    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;
    
    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess && _value is not null
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");
    
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null
            ? Success(value)
            : Failure<TValue>(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) =>
        Failure<TValue>(error);
}