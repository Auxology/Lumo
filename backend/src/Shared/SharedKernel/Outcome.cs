namespace SharedKernel;

public class Outcome
{
    protected Outcome(bool isSuccess, Fault fault)
    {
        if (isSuccess && fault != Fault.None)
            throw new InvalidOperationException("A successful outcome cannot have a fault.");

        if (!isSuccess && fault == Fault.None)
            throw new InvalidOperationException("A failed outcome must have a fault.");

        IsSuccess = isSuccess;
        Fault = fault;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Fault Fault { get; }

    public static Outcome Success() =>
        new(true, Fault.None);

    public static Outcome<TValue> Success<TValue>(TValue value) =>
        new(value, true, Fault.None);

    public static Outcome Failure(Fault fault) =>
        new(false, fault);

    public static Outcome<TValue> Failure<TValue>(Fault fault) =>
        new(default, false, fault);


    public static implicit operator Outcome(Fault fault) =>
        fault == Fault.None ? Success() : Failure(fault);
}

public sealed class Outcome<TValue> : Outcome
{
    private readonly TValue? _value;

    internal Outcome(TValue? value, bool isSuccess, Fault fault)
        : base(isSuccess, fault)
    {
        _value = value;
    }

    public TValue Value => IsSuccess && _value is not null
        ? _value!
        : throw new InvalidOperationException("The value of a failed outcome cannot be accessed.");

    public static implicit operator Outcome<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Fault.NullValue);

    public static implicit operator Outcome<TValue>(Fault fault) =>
            Failure<TValue>(fault);
}