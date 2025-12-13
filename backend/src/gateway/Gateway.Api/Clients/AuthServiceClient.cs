using System.Text.Json;
using Gateway.Api.Options;
using Microsoft.Extensions.Options;
using Shared.Contracts.Authentication;
using SharedKernel.ResultPattern;

namespace Gateway.Api.Clients;

internal sealed class AuthServiceClient(
    IHttpClientFactory httpClientFactory,
    IOptions<AuthServiceClientOptions> authServiceClientOptions) : IAuthServiceClient
{
    private readonly AuthServiceClientOptions _authServiceClientOptions = authServiceClientOptions.Value;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public async Task<Result<VerifyLoginResponse>> CallVerifyLoginAsync(CreateSessionRequest request, CancellationToken cancellationToken = default)
    {
        HttpClient httpClient = httpClientFactory.CreateClient(_authServiceClientOptions.HttpClientName);

        HttpResponseMessage response = await httpClient.PostAsJsonAsync(_authServiceClientOptions.VerifyLoginEndpoint,
            request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            VerifyLoginResponse? verifyLoginResponse =
                await response.Content.ReadFromJsonAsync<VerifyLoginResponse>(cancellationToken);

            if (verifyLoginResponse is null)
                throw new InvalidOperationException("Failed to deserialize the verify login response.");

            return verifyLoginResponse;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        ApiErrorResponseDto? apiErrorResponseDto = JsonSerializer.Deserialize<ApiErrorResponseDto>(content, JsonSerializerOptions);

        if (apiErrorResponseDto is null)
            throw new InvalidOperationException("Failed to deserialize the error response from Auth Service.");

        return apiErrorResponseDto.ToError();
    }
}
