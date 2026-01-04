namespace SharedKernel.Application.Authentication;

public interface IRequestContext
{
    string IpAddress { get; }

    string UserAgent { get; }

    string Timezone { get; }

    string Language { get; }

    string NormalizedBrowser { get; }

    string NormalizedOs { get; }

    string CorrelationId { get; }
}