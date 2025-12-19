using Auth.Domain.Constants;
using SharedKernel;

namespace Auth.Domain.Faults;

public static class UserFaults
{
    public static Fault DisplayNameRequiredForCreation => Fault.Validation
    (
        title: "User.DisplayNameRequiredForCreation",
        detail: "A display name is required to create your account."
    );

    public static Fault DisplayNameTooLongForCreation => Fault.Validation
    (
        title: "User.DisplayNameTooLongForCreation",
        detail: $"Your display name must not exceed {UserConstants.MaxDisplayNameLength} characters."
    );

    public static Fault EmailAddressRequiredForCreation => Fault.Validation
    (
        title: "User.EmailAddressRequiredForCreation",
        detail: "An email address is required to create your account."
    );

    public static Fault DisplayNameRequiredForUpdate => Fault.Validation
    (
        title: "User.DisplayNameRequiredForUpdate",
        detail: "The new display name cannot be empty."
    );

    public static Fault DisplayNameTooLongForUpdate => Fault.Validation
    (
        title: "User.DisplayNameTooLongForUpdate",
        detail: $"The new display name must not exceed {UserConstants.MaxDisplayNameLength} characters."
    );

    public static Fault AvatarKeyRequiredForUpdate => Fault.Validation
    (
        title: "User.AvatarKeyRequiredForUpdate",
        detail: "Please provide a valid avatar to update your profile picture."
    );
}