namespace SharedKernel.ResultPattern;

public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base
        (
            Title: "Validation.General",
            Detail: "One or more validation errors occurred",
            ErrorType.Validation
        )
    {
        Errors = errors;
    }

    public IReadOnlyCollection<Error> Errors { get; }
}
