using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;
using StuMoov.Models.UserModel.UserRecords;
using Supabase;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    readonly Supabase.Client _db;
    readonly IConfiguration _cfg;

    public AuthController(Supabase.Client db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    [HttpPost("signup"), AllowAnonymous]
    public async Task<IActionResult> Signup([FromBody] SignupDto dto)
    {
        var idToken = ExtractBearer();
        var fToken = await FirebaseAuth.DefaultInstance
                         .VerifyIdTokenAsync(idToken);
        var uid = fToken.Uid;
        var email = fToken.Claims["email"]!.ToString();
        var name = fToken.Claims.ContainsKey("name")
                      ? fToken.Claims["name"]!.ToString()
                      : email;

        // Create a new GUID for the user
        var userId = Guid.NewGuid();

        // Create the appropriate user type based on the role
        User user = dto.Role switch
        {
            UserRole.LENDER => new Lender(userId, name, email, uid),
            _ => new Renter(userId, name, email, uid)
        };

        // For Supabase, we need to use the appropriate type for database operations
        // but we can still use polymorphism in our code
        if (user is Lender lender)
        {
            try
            {
                var insert = await _db.From<Lender>().Insert(lender);
                if (!insert.Models.Any())
                    return BadRequest("Couldn't create lender user.");
            }
            catch (Supabase.Postgrest.Exceptions.PostgrestException pgEx)
            {
                // Add appropriate reason if available
                pgEx.AddReason();

                return StatusCode(pgEx.StatusCode, $"Database error: {pgEx.Message}");
            }
        }
        else if (user is Renter renter)
        {
            try
            {
                var insert = await _db.From<Renter>().Insert(renter);
                if (!insert.Models.Any())
                    return BadRequest("Couldn't create renter user.");
            }
            catch (Supabase.Postgrest.Exceptions.PostgrestException pgEx)
            {
                // Add appropriate reason if available
                pgEx.AddReason();

                return StatusCode(pgEx.StatusCode, $"Database error: {pgEx.Message}");
            }
        }

        var userRole = user switch
        {
            Lender _ => UserRole.LENDER,
            Renter _ => UserRole.RENTER,
            _ => throw new InvalidOperationException("Unknown user type")
        };

        var jwt = GenerateJwt(user.Id.ToString(), user.Email, userRole.ToString());
        return Ok(new
        {
            uid,
            email = user.Email,
            displayName = user.DisplayName,
            role = userRole.ToString(),
            token = jwt
        });
    }

    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login()
    {
        var idToken = ExtractBearer();
        var fToken = await FirebaseAuth.DefaultInstance
                         .VerifyIdTokenAsync(idToken);
        var uid = fToken.Uid;

        // Instead of using Renter/Lender tables to determine role, use UserRecords
        var userRecordResponse = await _db.From<UserRecords>()
                   .Where(u => u.FirebaseUid == uid)
                   .Get();

        if (userRecordResponse.Models.Any())
        {
            var userRecord = userRecordResponse.Models.First();

            var jwt = GenerateJwt(userRecord.Id.ToString(), userRecord.Email, userRecord.Role.ToString());

            return Ok(new
            {
                uid,
                email = userRecord.Email,
                displayName = userRecord.DisplayName,
                role = userRecord.Role.ToString(),
                token = jwt
            });
        }

        return Unauthorized();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        // Get user ID from the JWT claims
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Invalid authentication token");

        try
        {
            // Find user in db
            var meResponse = await _db.From<UserRecords>()
                                  .Where(u => u.Id == Guid.Parse(userId))
                                  .Get();

            if (meResponse == null || !meResponse.Models.Any())
                return NotFound("User not found");

            var userData = meResponse.Models.First();

            return Ok(new
            {
                id = userData.Id,
                email = userData.Email,
                displayName = userData.DisplayName,
                role = userData.Role,
                isEmailVerified = userData.IsEmailVerified
            });
        }
        catch (Supabase.Postgrest.Exceptions.PostgrestException pgEx)
        {
            // Add appropriate reason if available
            pgEx.AddReason();

            return StatusCode(pgEx.StatusCode, $"Database error: {pgEx.Message}");
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while retrieving user data");
        }
    }

    private string ExtractBearer()
    {
        var h = Request.Headers["Authorization"].ToString();
        if (!h.StartsWith("Bearer ")) throw new UnauthorizedAccessException();
        return h["Bearer ".Length..];
    }

    private string GenerateJwt(string sub, string email, string role)
    {
        var section = _cfg.GetSection("Jwt");
        // read it as an int, default to 60 if missing
        var expireMinutes = section.GetValue<int>("ExpiryMinutes", 60);

        var keyBytes = Encoding.UTF8.GetBytes(section["Key"]!);
        var creds = new SigningCredentials(
                         new SymmetricSecurityKey(keyBytes),
                         SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, sub),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role),
        };

        var token = new JwtSecurityToken(
          issuer: section["Issuer"],
          audience: section["Audience"],
          claims: claims,
          expires: DateTime.UtcNow.AddMinutes(expireMinutes),
          signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class SignupDto { public UserRole Role { get; set; } }
}
