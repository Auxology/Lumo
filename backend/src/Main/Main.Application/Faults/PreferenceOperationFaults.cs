using SharedKernel;

namespace Main.Application.Faults;

internal static class PreferenceOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "Preference.NotFound",
        detail: "The user preference was not found."
    );

    internal static readonly Fault Conflict = Fault.Conflict
    (
        title: "Preference.Conflict",
        detail: "A concurrent operation conflicted with this request. Please try again."
    );

    internal static readonly Fault ModelNotFound = Fault.NotFound
    (
        title: "Preference.ModelNotFound",
        detail: "The specified model was not found in the model registry."
    );
}