using SharedKernel.ResultPattern;

namespace Auth.Application.Errors;

public static class UserOperationErrors
{
    public static Error EmailAlreadyExists => Error.Conflict
    (
        title: "User.EmailAlreadyExists",
        detail: "A user with the provided email address already exists."
    );
    
    public static Error UserNotFound => Error.NotFound
    (
        title: "User.NotFound",
        detail: "The specified user was not found."
    );

    public static Error InvalidCredentials => Error.Unauthorized
    (
        title: "User.InvalidCredentials",
        detail: "The provided credentials are invalid."
    );
    
    public static Error AvatarFileNotFound => Error.NotFound
    (
        title: "User.AvatarFileNotFound",
        detail: "The specified avatar file was not found in storage."
    );
    
    public static Error AvatarKeyDoesNotBelongToUser => Error.Forbidden
    (
        title: "User.AvatarKeyDoesNotBelongToUser",
        detail: "The provided avatar key does not belong to the user."
    );
}