namespace Auth.Infrastructure.Options;

public sealed class AuthDatabaseOptions
{
    public const string SectionName = "AuthDatabase";
    
    public string ConnectionString { get; init; } = string.Empty;
    
    public bool EnableSensitiveDataLogging { get; init; }
    
    public int CommandTimeout { get; init; } = 30;
}