namespace SharedKernel;

public record Fault(string Title, string Detail, FaultKind Kind)
{
    public static readonly Fault None = new(string.Empty, string.Empty, FaultKind.None);

    public static readonly Fault NullValue = new
    (
        Title: "General.Null",
        Detail: "Null value was provided",
        Kind: FaultKind.Validation
    );

    public static Fault Failure(string title, string detail) =>
        new(title, detail, FaultKind.Failure);

    public static Fault Problem(string title, string detail) =>
        new(title, detail, FaultKind.Problem);

    public static Fault Validation(string title, string detail) =>
        new(title, detail, FaultKind.Validation);

    public static Fault Conflict(string title, string detail) =>
        new(title, detail, FaultKind.Conflict);

    public static Fault NotFound(string title, string detail) =>
        new(title, detail, FaultKind.NotFound);

    public static Fault Unauthorized(string title, string detail) =>
        new(title, detail, FaultKind.Unauthorized);

    public static Fault Forbidden(string title, string detail) =>
        new(title, detail, FaultKind.Forbidden);

    public static Fault TooManyRequests(string title, string detail) =>
        new(title, detail, FaultKind.TooManyRequests);
}