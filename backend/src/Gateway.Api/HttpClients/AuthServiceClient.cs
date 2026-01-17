using System.Text.Json;

using Contracts.Requests;
using Contracts.Responses;

using Gateway.Api.Faults;

using SharedKernel;
using SharedKernel.Api.DTOs;
using SharedKernel.Api.Extensions;

namespace Gateway.Api.HttpClients;

internal sealed class AuthServiceClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IAuthServiceClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Outcome<VerifyLoginApiResponse>> VerifyLoginAsync(VerifyLoginApiRequest request,
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, "v1/api/login-requests/verify");
        requestMessage.Content = JsonContent.Create(request);

        ForwardClientHeaders(requestMessage);

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage, cancellationToken);

        return await ProcessResponseAsync<VerifyLoginApiResponse>(response, cancellationToken);
    }

    public async Task<Outcome<RefreshSessionApiResponse>> RefreshSessionAsync(RefreshSessionApiRequest request,
        CancellationToken cancellationToken = default)

    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Post, "v1/api/sessions/refresh")
        {
            Content = JsonContent.Create(request)
        };

        ForwardClientHeaders(requestMessage);

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage, cancellationToken);

        return await ProcessResponseAsync<RefreshSessionApiResponse>(response, cancellationToken);
    }

    public async Task<Outcome> LogoutAsync(LogoutApiRequest request, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage requestMessage = new(HttpMethod.Delete, "v1/api/sessions");
        requestMessage.Content = JsonContent.Create(request);

        ForwardClientHeaders(requestMessage);

        HttpResponseMessage response = await httpClient.SendAsync(requestMessage, cancellationToken);

        return await ProcessResponseAsync<object>(response, cancellationToken);
    }

    private void ForwardClientHeaders(HttpRequestMessage requestMessage)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
            return;

        if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent))
            requestMessage.Headers.TryAddWithoutValidation("User-Agent", userAgent.ToString());

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            requestMessage.Headers.TryAddWithoutValidation("X-Forwarded-For", forwardedFor.ToString());
        }
        else if (httpContext.Connection.RemoteIpAddress is not null)
        {
            requestMessage.Headers.TryAddWithoutValidation("X-Forwarded-For",
                httpContext.Connection.RemoteIpAddress.ToString());
        }

        if (httpContext.Request.Headers.TryGetValue("X-Timezone", out var timezone))
            requestMessage.Headers.TryAddWithoutValidation("X-Timezone", timezone.ToString());

        if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var acceptLanguage))
            requestMessage.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage.ToString());

        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            requestMessage.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId.ToString());
    }

    private static async Task<Outcome<T>> ProcessResponseAsync<T>(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            T? tResponse = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

            if (tResponse is null)
                return AuthFaults.FailedToDeserialize;

            return tResponse;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        ApiProblemDetailsDto? problemDetailsDto =
            JsonSerializer.Deserialize<ApiProblemDetailsDto>(content, JsonSerializerOptions);

        if (problemDetailsDto is null)
            return AuthFaults.FailedToDeserialize;

        return problemDetailsDto.ToFault();
    }
}