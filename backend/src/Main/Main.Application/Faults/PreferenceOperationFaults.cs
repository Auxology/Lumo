using SharedKernel;

namespace Main.Application.Faults;

internal static class PreferenceOperationFaults
{
    internal static readonly Fault NotFound = Fault.NotFound
    (
        title: "Preference.NotFound",
        detail: "The user preference was not found."
    );
}