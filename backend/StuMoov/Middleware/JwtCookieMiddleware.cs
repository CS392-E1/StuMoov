using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace StuMoov.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if there's a JWT cookie
            if (context.Request.Cookies.TryGetValue("auth_token", out string? token))
            {
                // If the request doesn't already have an Authorization header, set it from the cookie
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

    // Extension method for middleware registration
    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}