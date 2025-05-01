/**
 * AuthController.cs
 * 
 * Handles user authentication endpoints including registration, login, verification,
 * and logout operations. Also creates Stripe Connect accounts for lenders during registration.
 */

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
    private readonly AuthService _authService;      // Service for authentication operations
    private readonly StripeService _stripeService;  // Service for Stripe payment integration
    private readonly IConfiguration _configuration; // Application configuration
    private readonly ILogger<AuthController> _logger; // Logging service

    /// <summary>
    /// Initialize the AuthController with required services
    /// </summary>
    public AuthController(
        AuthService authService,
        StripeService stripeService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _stripeService = stripeService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user as a lender and creates a Stripe Connect account
    /// </summary>
    /// <returns>Registration result with user details and token</returns>
    /// <route>POST: api/auth/register/lender</route>
    [HttpPost("register/lender"), AllowAnonymous]
    public async Task<IActionResult> RegisterLender()
    {
        // Extract Firebase ID token from Authorization header
        var idToken = ExtractBearer();

        // Register user with LENDER role
        Response response = await _authService.SignupAsync(idToken, UserRole.LENDER);

        // Set JWT token in HTTP-only cookie
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
                    // Create Stripe Connect account for payment processing
                    await _stripeService.CreateConnectAccountForLenderAsync(userId.Value);
                    _logger.LogInformation("Successfully created Stripe Connect account for Lender {UserId} from AuthController", userId.Value);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the registration process
                    // This ensures user can still register even if Stripe account creation fails
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

    /// <summary>
    /// Registers a new user as a renter
    /// </summary>
    /// <returns>Registration result with user details and token</returns>
    /// <route>POST: api/auth/register/renter</route>
    [HttpPost("register/renter"), AllowAnonymous]
    public async Task<IActionResult> RegisterRenter()
    {
        // Extract Firebase ID token from Authorization header
        var idToken = ExtractBearer();

        // Register user with RENTER role
        var response = await _authService.SignupAsync(idToken, UserRole.RENTER);

        // Set JWT token in HTTP-only cookie
        HandleAuthResponse(response);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Logs in an existing user
    /// </summary>
    /// <returns>Login result with user details and token</returns>
    /// <route>POST: api/auth/login</route>
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        // Extract Firebase ID token from Authorization header
        var idToken = ExtractBearer();

        // Authenticate user with provided token
        var response = await _authService.LoginAsync(idToken);

        // Set JWT token in HTTP-only cookie
        HandleAuthResponse(response);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Verifies if the current user's token is valid
    /// </summary>
    /// <returns>Verification result with user ID if successful</returns>
    /// <route>GET: api/auth/verify</route>
    [HttpGet("verify")]
    [Authorize] // Requires valid JWT token
    public IActionResult Verify()
    {
        // Look for user ID in the nameidentifier claim instead of sub
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        return Ok(new Response(200, "User verified successfully", new { userId }));
    }

    /// <summary>
    /// Logs out the current user by clearing authentication cookie
    /// </summary>
    /// <returns>Success message</returns>
    /// <route>POST: api/auth/logout</route>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Clear the auth cookie with secure settings
        Response.Cookies.Delete("auth_token", new CookieOptions
        {
            HttpOnly = true,     // Prevents JavaScript access
            Secure = true,       // Only sent over HTTPS
            SameSite = SameSiteMode.Strict,  // Prevents CSRF attacks
            Path = "/"           // Available across the entire site
        });

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Extracts the bearer token from the Authorization header
    /// </summary>
    /// <returns>The token string without the "Bearer " prefix</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when Authorization header is invalid</exception>
    private string ExtractBearer()
    {
        var h = Request.Headers["Authorization"].ToString();
        if (!h.StartsWith("Bearer ")) throw new UnauthorizedAccessException();
        return h["Bearer ".Length..]; // C# range operator to remove "Bearer " prefix
    }

    /// <summary>
    /// Sets the JWT authentication token as a secure HTTP-only cookie
    /// </summary>
    /// <param name="token">The JWT token to set</param>
    private void SetAuthCookie(string token)
    {
        // Set JWT as HttpOnly cookie with security attributes
        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,     // Prevents JavaScript access to mitigate XSS attacks
            Secure = true,       // Only sent over HTTPS connections
            SameSite = SameSiteMode.Strict,  // Prevents CSRF attacks
            Expires = DateTimeOffset.Now.AddDays(1),  // Cookie valid for 1 day
            Path = "/"           // Available across the entire site
        });
    }

    /// <summary>
    /// Processes authentication response and sets JWT cookie if login/registration was successful
    /// </summary>
    /// <param name="response">The authentication response containing token data</param>
    private void HandleAuthResponse(Response response)
    {
        // Check if authentication was successful and response contains data
        if ((response.Status == StatusCodes.Status200OK ||
             response.Status == StatusCodes.Status201Created) &&
            response.Data != null)
        {
            // Extract token from dynamic response data using reflection
            var token = response.Data.GetType().GetProperty("token")?.GetValue(response.Data, null) as string;

            if (!string.IsNullOrEmpty(token))
            {
                SetAuthCookie(token);
            }
        }
    }
}