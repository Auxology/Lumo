namespace SharedKernel;

public sealed record ValidationFault : Fault
{
    public ValidationFault(IEnumerable<Fault> faults)
        : base
        (
            Title: "General.Validation",
            Detail: "One or more validation faults occurred.",
            Kind: FaultKind.Validation
        )
    {
        Faults = [..faults];
    }

    public IReadOnlyList<Fault> Faults { get; }
}