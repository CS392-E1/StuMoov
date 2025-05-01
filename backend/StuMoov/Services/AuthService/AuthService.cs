/**
 * AuthService.cs
 *
 * Handles user authentication and authorization for the StuMoov application.
 * Provides methods for user signup and login, integrating with Firebase Authentication
 * and generating JWT tokens for secure access. Utilizes dependency injection for
 * data access, configuration, and logging.
 */

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
    /// <summary>
    /// Service responsible for handling user authentication, including signup and login operations.
    /// Integrates with Firebase Authentication and generates JWT tokens for secure access.
    /// </summary>
    public class AuthService
    {
        private readonly UserDao _userDao;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthService with required dependencies.
        /// </summary>
        /// <param name="userDao">Data access object for user-related operations.</param>
        /// <param name="configuration">Configuration for accessing JWT settings.</param>
        /// <param name="logger">Logger for recording authentication-related events.</param>
        public AuthService(UserDao userDao, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userDao = userDao;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with the provided Firebase ID token and role.
        /// Verifies the token, creates a user in the database, and generates a JWT token.
        /// </summary>
        /// <param name="idToken">Firebase ID token for authentication.</param>
        /// <param name="role">The role of the user (Lender or Renter).</param>
        /// <returns>A Response object containing the status, message, and user data with JWT token.</returns>
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
                    // Attempt to delete the user from Firebase if database registration fails
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

        /// <summary>
        /// Authenticates a user with the provided Firebase ID token.
        /// Verifies the token, retrieves the user from the database, and generates a JWT token.
        /// </summary>
        /// <param name="idToken">Firebase ID token for authentication.</param>
        /// <returns>A Response object containing the status, message, and user data with JWT token.</returns>
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

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// Includes user ID, email, and role in the token claims.
        /// </summary>
        /// <param name="sub">The subject (user ID) for the JWT token.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="role">The user's role (e.g., Lender, Renter).</param>
        /// <returns>The generated JWT token as a string.</returns>
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

    /// <summary>
    /// Data Transfer Object (DTO) for signup requests.
    /// Specifies the role of the user being registered.
    /// </summary>
    public class SignupDto
    {
        /// <summary>
        /// The role of the user (Lender or Renter).
        /// </summary>
        public UserRole Role { get; set; }
    }
}