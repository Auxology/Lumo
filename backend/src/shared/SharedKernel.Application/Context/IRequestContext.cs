namespace SharedKernel.Application.Context;

public interface IRequestContext
{
    string CorrelationId { get; }
    
    string? ClientIp { get; }
    
    string? UserAgent { get; }

    string? Browser { get; }
    
    string? OperatingSystem { get; }
    
    string? Device { get; }
    
    bool IsMobile { get; }
    
    string? RequestPath { get; }
    
    string? RequestMethod { get; }
}