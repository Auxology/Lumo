namespace Auth.Domain.ValueObjects;

public readonly record struct RecoverKeyInput
{
    public string Identifier { get; }

    public string VerifierHash { get; }

    private RecoverKeyInput(string identifier, string verifierHash)
    {
        Identifier = identifier;
        VerifierHash = verifierHash;
    }

    public static RecoverKeyInput Create(string identifier, string verifierHash) =>
        new(identifier, verifierHash);
}