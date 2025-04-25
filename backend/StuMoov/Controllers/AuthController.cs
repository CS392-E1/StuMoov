using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StuMoov.Models.UserModel.Enums;
using StuMoov.Services.AuthService;
using StuMoov.Services.StripeService;
using System.Security.Claims;
using StuMoov.Models;
using StuMoov.Models.UserModel;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly StripeService _stripeService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, StripeService stripeService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _authService = authService;
        _stripeService = stripeService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register/lender"), AllowAnonymous]
    public async Task<IActionResult> RegisterLender()
    {
        var idToken = ExtractBearer();
        Response response = await _authService.SignupAsync(idToken, UserRole.LENDER);

        // Set HttpOnly cookie with the JWT
        HandleAuthResponse(response);

        // Create Stripe Connect account for the lender if registration was successful
        if (response.Status == StatusCodes.Status201Created && response.Data != null)
        {
            dynamic responseData = response.Data;
            User user = responseData.createdUser;
            Guid? userId = user?.Id;

            if (userId.HasValue)
            {
                try
                {
                    // Create Stripe Connect account
                    await _stripeService.CreateConnectAccountForLenderAsync(userId.Value);
                    _logger.LogInformation("Successfully created Stripe Connect account for Lender {UserId} from AuthController", userId.Value);

                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the registration
                    _logger.LogError(ex, "Failed to create Stripe Connect account during registration for user {UserId} from AuthController", userId.Value);
                }
            }
            else
            {
                _logger.LogWarning("Could not extract user ID from successful registration response to create Stripe account.");
            }
        }

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
