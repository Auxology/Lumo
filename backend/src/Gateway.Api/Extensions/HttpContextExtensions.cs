using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using SharedKernel.Infrastructure.Options;

namespace Gateway.Api.Extensions;

internal static class HttpContextExtensions
{
    private const string CookieName = "refresh_token";

    public static void SetRefreshTokenCookie(this HttpContext httpContext, string refreshToken)
    {
        JwtOptions jwtOptions = httpContext.RequestServices.GetRequiredService<IOptions<JwtOptions>>().Value;
        IWebHostEnvironment environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();

        bool isDevelopment = environment.IsDevelopment();

        httpContext.Response.Cookies.Append(CookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/",
            MaxAge = jwtOptions.RefreshTokenExpiration
        });
    }

    public static void RemoveRefreshTokenCookie(this HttpContext httpContext)
    {
        IWebHostEnvironment environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        bool isDevelopment = environment.IsDevelopment();

        httpContext.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = isDevelopment ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/"
        });
    }

    public static string? GetRefreshTokenCookie(this HttpContext httpContext) =>
        httpContext.Request.Cookies[CookieName];
}