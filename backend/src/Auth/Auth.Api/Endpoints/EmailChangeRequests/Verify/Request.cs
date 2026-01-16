namespace Auth.Api.Endpoints.EmailChangeRequests.Verify;

internal sealed record Request
(
    string TokenKey,  // From route parameter
    string OtpToken   // From request body
);