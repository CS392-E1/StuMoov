using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models.UserModel.Enums;
using StuMoov.Services.AuthService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using StuMoov.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/lender"), AllowAnonymous]
    public async Task<IActionResult> RegisterLender()
    {
        var idToken = ExtractBearer();
        var response = await _authService.SignupAsync(idToken, UserRole.LENDER);

        // Set HttpOnly cookie with the JWT
        HandleAuthResponse(response);

        return StatusCode(response.Status, response);
    }

    [HttpPost("register/renter"), AllowAnonymous]
    public async Task<IActionResult> RegisterRenter()
    {
        var idToken = ExtractBearer();
        var response = await _authService.SignupAsync(idToken, UserRole.RENTER);

        // Set HttpOnly cookie with the JWT
        HandleAuthResponse(response);

        return StatusCode(response.Status, response);
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        var idToken = ExtractBearer();
        var response = await _authService.LoginAsync(idToken);

        HandleAuthResponse(response);

        return StatusCode(response.Status, response);
    }

    [HttpGet("verify")]
    [Authorize]
    public IActionResult Verify()
    {
        // Look for user ID in the nameidentifier claim instead of sub
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        return Ok(new { userId });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the auth cookie
        Response.Cookies.Delete("auth_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        return Ok(new { message = "Logged out successfully" });
    }

    private string ExtractBearer()
    {
        var h = Request.Headers["Authorization"].ToString();
        if (!h.StartsWith("Bearer ")) throw new UnauthorizedAccessException();
        return h["Bearer ".Length..];
    }

    private void SetAuthCookie(string token)
    {
        // Set JWT as HttpOnly cookie
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Only sent over HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.Now.AddDays(7), // Match your token expiration
            Path = "/"
        });
    }

    private void HandleAuthResponse(Response response)
    {
        if ((response.Status == StatusCodes.Status200OK ||
             response.Status == StatusCodes.Status201Created) &&
            response.Data != null)
        {
            var token = response.Data.GetType().GetProperty("token")?.GetValue(response.Data, null) as string;
            if (!string.IsNullOrEmpty(token))
            {
                SetAuthCookie(token);
            }
        }
    }
}
