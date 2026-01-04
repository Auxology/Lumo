using System.Text.Json;

using Contracts.Requests;
using Contracts.Responses;

using Gateway.Api.Faults;

using SharedKernel;
using SharedKernel.Api.DTOs;
using SharedKernel.Api.Extensions;

namespace Gateway.Api.HttpClients;

internal sealed class AuthServiceClient(HttpClient httpClient) : IAuthServiceClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Outcome<VerifyLoginApiResponse>> VerifyLoginAsync(VerifyLoginApiRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync
        (
            "api/v1/login-requests/verify",
            request,
            cancellationToken
        );

        return await ProcessResponseAsync<VerifyLoginApiResponse>(response, cancellationToken);
    }

    public async Task<Outcome<RefreshSessionApiResponse>> RefreshSessionAsync(RefreshSessionApiRequest request,
        CancellationToken cancellationToken = default)

    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync
        (
            "api/v1/sessions/refresh",
            request,
            cancellationToken
        );

        return await ProcessResponseAsync<RefreshSessionApiResponse>(response, cancellationToken);
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