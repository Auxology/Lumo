using SharedKernel.ResultPattern;

namespace Auth.Application.Errors;

public static class UserOperationErrors
{
    public static Error EmailAlreadyExists => Error.Conflict
    (
        title: "User.EmailAlreadyExists",
        detail: "A user with the provided email address already exists."
    );
}