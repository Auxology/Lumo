namespace Auth.Api.Endpoints.Sessions.RevokeSessions;

internal sealed record Request(IReadOnlyList<string> SessionIds);