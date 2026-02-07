using SharedKernel;

namespace Main.Domain.Faults;

public static class FavoriteModelFaults
{
    public static readonly Fault FavoriteModelIdRequired = Fault.Validation
    (
        title: "FavoriteModel.FavoriteModelIdRequired",
        detail: "FavoriteModelId is required."
    );

    public static readonly Fault PreferenceIdRequired = Fault.Validation
    (
        title: "FavoriteModel.PreferenceIdRequired",
        detail: "PreferenceId is required."
    );

    public static readonly Fault ModelIdRequired = Fault.Validation
    (
        title: "FavoriteModel.ModelIdRequired",
        detail: "ModelId is required."
    );
}