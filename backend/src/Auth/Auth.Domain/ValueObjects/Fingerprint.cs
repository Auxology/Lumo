using System.Security.Cryptography;
using System.Text;
using SharedKernel;

namespace Auth.Domain.ValueObjects;

public readonly record struct Fingerprint
{
    public string IpAddress { get; }
    
    public string UserAgent { get; }
    
    public string Timezone { get; }
    
    public string Language { get; }
    
    public string ComputedHash { get; }
    
    public string NormalizedBrowser { get; }
    
    public string NormalizedOs { get; }

    private Fingerprint
    (
        string ipAddress,
        string userAgent,
        string timezone,
        string language,
        string computedHash,
        string normalizedBrowser,
        string normalizedOs
    )
    {
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Timezone = timezone;
        Language = language;
        ComputedHash = computedHash;
        NormalizedBrowser = normalizedBrowser;
        NormalizedOs = normalizedOs;
    }

    public static Outcome<Fingerprint> Create
    (
        string ipAddress,
        string userAgent,
        string timezone,
        string language,
        string normalizedBrowser,
        string normalizedOs
    )
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return Faults.IpAddressRequired;
        
        if (string.IsNullOrWhiteSpace(userAgent))
            return Faults.UserAgentRequired;
        
        if (string.IsNullOrWhiteSpace(timezone))
            return Faults.TimezoneRequired;
        
        if (string.IsNullOrWhiteSpace(language))
            return Faults.LanguageRequired;
        
        if (string.IsNullOrWhiteSpace(normalizedBrowser))
            return Faults.NormalizedBrowserRequired;
        
        if (string.IsNullOrWhiteSpace(normalizedOs))
            return Faults.NormalizedOsRequired;
        
        string computedHash = ComputeHash(ipAddress, userAgent, timezone, language);

        Fingerprint fingerprint = new
        (
            ipAddress: ipAddress,
            userAgent: userAgent,
            timezone: timezone,
            language: language,
            computedHash: computedHash,
            normalizedBrowser: normalizedBrowser,
            normalizedOs: normalizedOs
        );
        
        return fingerprint;
    }

    private static string ComputeHash
    (
        string ipAddress,
        string userAgent,
        string timezone,
        string language
    )
    {
        string rawData = $"{ipAddress}|{userAgent}|{timezone}|{language}";
        byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        
        return Convert.ToBase64String(hashedBytes);
    }
    
    private static class Faults
    {
        public static readonly Fault IpAddressRequired = Fault.Validation
        (
            title: "Fingerprint.IpAddressRequired",
            detail: "An IP address is required for the fingerprint."
        );
        
        public static readonly Fault UserAgentRequired = Fault.Validation
        (
            title: "Fingerprint.UserAgentRequired",
            detail: "A user agent is required for the fingerprint."
        );
        
        public static readonly Fault TimezoneRequired = Fault.Validation
        (
            title: "Fingerprint.TimezoneRequired",
            detail: "A timezone is required for the fingerprint."
        );
        
        public static readonly Fault LanguageRequired = Fault.Validation
        (
            title: "Fingerprint.LanguageRequired",
            detail: "A language is required for the fingerprint."
        );
        
        public static readonly Fault NormalizedBrowserRequired = Fault.Validation
        (
            title: "Fingerprint.NormalizedBrowserRequired",
            detail: "A normalized browser is required for the fingerprint."
        );
        
        public static readonly Fault NormalizedOsRequired = Fault.Validation
        (
            title: "Fingerprint.NormalizedOsRequired",
            detail: "A normalized operating system is required for the fingerprint."
        );
    }
}