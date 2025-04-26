using FirebaseAdmin.Auth;
using Microsoft.IdentityModel.Tokens;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StuMoov.Services.AuthService
{
    public class AuthService
    {
        private readonly UserDao _userDao;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserDao userDao, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userDao = userDao;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<Response> SignupAsync(string idToken, UserRole role)
        {
            try
            {
                // Verify Firebase token
                var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = firebaseToken.Uid;
                var email = firebaseToken.Claims["email"].ToString()!;
                var name = firebaseToken.Claims.ContainsKey("name")
                    ? firebaseToken.Claims["name"].ToString()!
                    : email;

                // Check if user already exists
                var existingUser = await _userDao.GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    return new Response(
                        StatusCodes.Status409Conflict,
                        "User with this email already exists",
                        null
                    );
                }

                // Create a new GUID for the user
                var userId = Guid.NewGuid();

                // Create the appropriate user type based on the role
                User user = role switch
                {
                    UserRole.LENDER => new Lender(userId, name, email, uid),
                    _ => new Renter(userId, name, email, uid)
                };

                // Add user to database
                User? createdUser = await _userDao.AddUserAsync(user);

                if (createdUser == null)
                {
                    // This try/catch deletes the user from Firebase if the user was not created in the database
                    try
                    {
                        await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
                    }
                    catch (FirebaseAuthException deleteEx)
                    {

                        _logger.LogError($"Failed to delete Firebase user {uid} after DB registration failure: {deleteEx.Message}");
                    }

                    return new Response(
                        StatusCodes.Status500InternalServerError,
                        "Failed to register user in the database. Deleted user from Firebase.",
                        null
                    );
                }

                // Generate JWT token
                var jwt = GenerateJwt(createdUser.Id.ToString(), createdUser.Email, role.ToString());

                return new Response(
                    StatusCodes.Status201Created,
                    "User registered successfully",
                    new
                    {
                        createdUser,
                        token = jwt
                    }
                );
            }
            catch (FirebaseAuthException ex)
            {
                return new Response(
                    StatusCodes.Status401Unauthorized,
                    $"Firebase authentication failed: {ex.Message}",
                    null
                );
            }
            catch (Exception ex)
            {
                return new Response(
                    StatusCodes.Status500InternalServerError,
                    $"An error occurred: {ex.Message}",
                    null
                );
            }
        }

        public async Task<Response> LoginAsync(string idToken)
        {
            try
            {
                // Verify Firebase token
                var firebaseToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                var uid = firebaseToken.Uid;

                // Find user in database
                var users = await _userDao.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.FirebaseUid == uid);

                if (user == null)
                {
                    return new Response(
                        StatusCodes.Status404NotFound,
                        "User not found",
                        null
                    );
                }

                // Determine user role
                var role = user switch
                {
                    Lender _ => UserRole.LENDER,
                    Renter _ => UserRole.RENTER,
                    _ => throw new InvalidOperationException("Unknown user type")
                };

                // Generate JWT token
                var jwt = GenerateJwt(user.Id.ToString(), user.Email, role.ToString());

                return new Response(
                    StatusCodes.Status200OK,
                    "Login successful",
                    new
                    {
                        uid,
                        email = user.Email,
                        displayName = user.DisplayName,
                        role = role.ToString(),
                        token = jwt
                    }
                );
            }
            catch (FirebaseAuthException ex)
            {
                return new Response(
                    StatusCodes.Status401Unauthorized,
                    $"Firebase authentication failed: {ex.Message}",
                    null
                );
            }
            catch (Exception ex)
            {
                return new Response(
                    StatusCodes.Status500InternalServerError,
                    $"An error occurred: {ex.Message}",
                    null
                );
            }
        }

        public string GenerateJwt(string sub, string email, string role)
        {
            var section = _configuration.GetSection("Jwt");
            var expireMinutes = section.GetValue<int>("ExpiryMinutes", 60);

            var keyBytes = Encoding.UTF8.GetBytes(section["Key"]!);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, sub),
                new Claim(ClaimTypes.NameIdentifier, sub),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
            };

            var token = new JwtSecurityToken(
                issuer: section["Issuer"],
                audience: section["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // DTO for signup requests
    public class SignupDto
    {
        public UserRole Role { get; set; }
    }
}
