namespace SharedKernel.ResultPattern;

public record Error(string Title, string Detail, ErrorType Type)
{
    public static Error None => new
    (
        Title: string.Empty,
        Detail: string.Empty,
        Type: ErrorType.None
    );

    public static Error NullValue => new
    (
        Title: "General.Null",
        Detail: "A null value was encountered where it is not allowed.",
        Type: ErrorType.Validation
    );
    
    public static Error Failure(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Failure
    );
    
    public static Error Problem(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Problem
    );
    
    public static Error NotFound(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.NotFound
    );
    
    public static Error Conflict(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Conflict
    );
    
    public static Error Validation(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Validation
    );
    
    public static Error Unauthorized(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Unauthorized
    );
    
    public static Error Forbidden(string title, string detail) => new
    (
        Title: title,
        Detail: detail,
        Type: ErrorType.Forbidden
    );
}