/**
 * JwtCookieMiddleware.cs
 *
 * Middleware to automatically populate the Authorization header from a JWT stored in a cookie.
 * If an "auth_token" cookie exists and no Authorization header is present, the middleware
 * adds a Bearer token header with the cookie value, then passes control to the next middleware.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace StuMoov.Middleware
{
    /// <summary>
    /// Middleware that checks for a JWT in an "auth_token" cookie and, if found,
    /// adds it as a Bearer Authorization header on the current HTTP request.
    /// </summary>
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;  // Delegate to the next middleware in the pipeline

        /// <summary>
        /// Constructs the middleware with a reference to the next RequestDelegate.
        /// </summary>
        /// <param name="next">The next middleware component in the pipeline</param>
        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware logic. If an "auth_token" cookie is present and
        /// the Authorization header is missing, adds an Authorization header with
        /// the Bearer token from the cookie.
        /// </summary>
        /// <param name="context">The current HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Try to retrieve JWT from the "auth_token" cookie
            if (context.Request.Cookies.TryGetValue("auth_token", out string? token))
            {
                // Only add header if it hasn't been provided already
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {token}");
                }
            }

            // Continue processing in the middleware pipeline
            await _next(context);
        }
    }

    /// <summary>
    /// Extension methods for registering JwtCookieMiddleware in the application pipeline.
    /// </summary>
    public static class JwtCookieMiddlewareExtensions
    {
        /// <summary>
        /// Adds JwtCookieMiddleware to the ASP.NET Core middleware pipeline.
        /// </summary>
        /// <param name="builder">The IApplicationBuilder to configure</param>
        /// <returns>The updated IApplicationBuilder</returns>
        public static IApplicationBuilder UseJwtCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}