using Auth.Domain.Constants;
using SharedKernel.Time;

namespace Gateway.Api.Extensions;

internal static class HttpContextExtensions
{
    private const string RefreshTokenCookieName = "refreshToken";

    public static void SetRefreshTokenCookie
    (
        this HttpContext httpContext,
        string refreshToken,
        IDateTimeProvider dateTimeProvider
    )
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/sessions",
            Expires = dateTimeProvider.UtcNow.AddDays(SessionConstants.ExpirationDays)
        };

        httpContext.Response.Cookies.Append
        (
            RefreshTokenCookieName,
            refreshToken,
            cookieOptions
        );
    }

    public static string? GetRefreshTokenFromCookie(this HttpContext httpContext)
    {
        httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out string? refreshToken);

        return refreshToken;
    }

    public static void RemoveRefreshTokenCookie(this HttpContext httpContext, IDateTimeProvider dateTimeProvider)
    {
        CookieOptions cookieOptions = new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/sessions",
            Expires = dateTimeProvider.UtcNow.AddDays(-1)
        };

        httpContext.Response.Cookies.Delete
        (
            RefreshTokenCookieName,
            cookieOptions
        );
    }
}
