/**
 * UserController.cs
 * 
 * Handles user management functionality including retrieving, creating,
 * updating, and deleting users. Provides endpoints for user-related operations.
 */

namespace StuMoov.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using StuMoov.Models;
    using StuMoov.Dao;
    using StuMoov.Models.UserModel;
    using StuMoov.Services.UserService;

    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly UserService _userService;  // Service for managing user operations

        /// <summary>
        /// Initialize the UserController with required dependencies.
        /// </summary>
        /// <param name="userDao">Data access object for user operations</param>
        public UserController(UserDao userDao)
        {
            _userService = new UserService(userDao);
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>List of all users wrapped in a Response object</returns>
        /// <route>GET: api/user</route>
        [HttpGet]
        public async Task<ActionResult> GetAllUsers()
        {
            var response = await _userService.GetAllUsersAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>User details wrapped in a Response object</returns>
        /// <route>GET: api/user/{id}</route>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserById(Guid id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <returns>User details wrapped in a Response object</returns>
        /// <route>GET: api/user/username/{username}</route>
        [HttpGet("username/{username}")]
        public async Task<ActionResult> GetUserByUsername(string username)
        {
            var response = await _userService.GetUserByUsernameAsync(username);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>User details wrapped in a Response object</returns>
        /// <route>GET: api/user/email/{email}</route>
        [HttpGet("email/{email}")]
        public async Task<ActionResult> GetUserByEmail(string email)
        {
            var response = await _userService.GetUserByEmailAsync(email);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all renters.
        /// </summary>
        /// <returns>List of renter users wrapped in a Response object</returns>
        /// <route>GET: api/user/renters</route>
        [HttpGet("renters")]
        public async Task<ActionResult> GetAllRenters()
        {
            var response = await _userService.GetAllRentersAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all lenders.
        /// </summary>
        /// <returns>List of lender users wrapped in a Response object</returns>
        /// <route>GET: api/user/lenders</route>
        [HttpGet("lenders")]
        public async Task<ActionResult> GetAllLenders()
        {
            var response = await _userService.GetAllLendersAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Registers a new renter user.
        /// </summary>
        /// <param name="renter">Renter model containing registration data</param>
        /// <returns>Result of the registration operation</returns>
        /// <route>POST: api/user/register/renter</route>
        [HttpPost("register/renter")]
        public async Task<ActionResult> RegisterRenter([FromBody] Renter renter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.RegisterUserAsync(renter);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Registers a new lender user.
        /// </summary>
        /// <param name="lender">Lender model containing registration data</param>
        /// <returns>Result of the registration operation</returns>
        /// <route>POST: api/user/register/lender</route>
        [HttpPost("register/lender")]
        public async Task<ActionResult> RegisterLender([FromBody] Lender lender)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _userService.RegisterUserAsync(lender);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="id">The unique identifier of the user to update</param>
        /// <param name="user">User model containing updated data</param>
        /// <returns>Result of the update operation</returns>
        /// <route>PUT: api/user/{id}</route>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid request data",
                    ModelState
                ));
            }

            if (id != user.Id)
            {
                return BadRequest(new Response(
                    StatusCodes.Status400BadRequest,
                    "ID in URL does not match ID in request body",
                    null
                ));
            }

            var updateResponse = await _userService.UpdateUserAsync(user);
            return StatusCode(updateResponse.Status, updateResponse);
        }

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to delete</param>
        /// <returns>Result of the delete operation</returns>
        /// <route>DELETE: api/user/{id}</route>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            var response = await _userService.DeleteUserAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves the total count of users.
        /// </summary>
        /// <returns>User count wrapped in a Response object</returns>
        /// <route>GET: api/user/count</route>
        [HttpGet("count")]
        public async Task<ActionResult> GetUserCount()
        {
            var response = await _userService.GetUserCountAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves renter details along with Stripe payment information.
        /// </summary>
        /// <param name="id">The unique identifier of the renter</param>
        /// <returns>Renter data with Stripe info wrapped in a Response object</returns>
        /// <route>GET: api/user/renter/{id}/stripe</route>
        [HttpGet("renter/{id}/stripe")]
        public async Task<ActionResult> GetRenterWithStripeInfo(Guid id)
        {
            var response = await _userService.GetRenterWithStripeInfoAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Checks whether a user exists by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user</param>
        /// <returns>Boolean indicating existence of the user</returns>
        /// <route>GET: api/user/exists/{id}</route>
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> UserExists(Guid id)
        {
            var exists = await _userService.UserExistsAsync(id);
            return Ok(exists);
        }
    }
}