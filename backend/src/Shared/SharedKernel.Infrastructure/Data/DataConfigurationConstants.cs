namespace SharedKernel.Infrastructure.Data;

public static class DataConfigurationConstants
{
    public const int DefaultStringMaxLength = 512;

    public const int MaxIpAddressLength = 45;

    public const int MaxUserAgentLength = 512;

    public const int MaxTimezoneLength = 64;

    public const int MaxLanguageLength = 16;

    public const int MaxNormalizedBrowserLength = 128;

    public const int MaxNormalizedOsLength = 128;

    public const string DefaultTimeColumnType = "timestamptz";
}