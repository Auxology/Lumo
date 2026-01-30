using SharedKernel;

namespace Main.Domain.Faults;

public static class PreferenceFaults
{
    public static readonly Fault UserIdRequired = Fault.Validation
    (
        title: "Preference.UserIdRequired",
        detail: "A user ID is required to create a preference."
    );

    public static readonly Fault MaxInstructionsReached = Fault.Validation
    (
        title: "Preference.MaxInstructionsReached",
        detail: "The maximum number of instructions has been reached."
    );

    public static readonly Fault InstructionNotFound = Fault.NotFound
    (
        title: "Preference.InstructionNotFound",
        detail: "The specified instruction was not found in the preference."
    );
}