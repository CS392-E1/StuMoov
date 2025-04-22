using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using StuMoov.Dao;
using StuMoov.Models.UserModel;
using StuMoov.Services.UserService;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserDao userDao)
        {
            _userService = new UserService(userDao);
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult> GetAllUsers()
        {
            Response response = await _userService.GetAllUsersAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUserById(Guid id)
        {
            Response response = await _userService.GetUserByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/username/{username}
        [HttpGet("username/{username}")]
        public async Task<ActionResult> GetUserByUsername(string username)
        {
            Response response = await _userService.GetUserByUsernameAsync(username);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/email/{email}
        [HttpGet("email/{email}")]
        public async Task<ActionResult> GetUserByEmail(string email)
        {
            Response response = await _userService.GetUserByEmailAsync(email);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/renters
        [HttpGet("renters")]
        public async Task<ActionResult> GetAllRenters()
        {
            Response response = await _userService.GetAllRentersAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/user/lenders
        [HttpGet("lenders")]
        public async Task<ActionResult> GetAllLenders()
        {
            Response response = await _userService.GetAllLendersAsync();
            return StatusCode(response.Status, response);
        }

        // POST: api/user/register/renter
        [HttpPost("register/renter")]
        public async Task<ActionResult> RegisterRenter([FromBody] Renter renter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = await _userService.RegisterUserAsync(renter);
            return StatusCode(response.Status, response);
        }

        // POST: api/user/register/lender
        [HttpPost("register/lender")]
        public async Task<ActionResult> RegisterLender([FromBody] Lender lender)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = await _userService.RegisterUserAsync(lender);
            return StatusCode(response.Status, response);
        }

        // PUT: api/user/{id}
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

            Response updateResponse = await _userService.UpdateUserAsync(user);
            return StatusCode(updateResponse.Status, updateResponse);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            Response response = await _userService.DeleteUserAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/count
        [HttpGet("count")]
        public async Task<ActionResult> GetUserCount()
        {
            Response response = await _userService.GetUserCountAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/user/renter/{id}/stripe
        [HttpGet("renter/{id}/stripe")]
        public async Task<ActionResult> GetRenterWithStripeInfo(Guid id)
        {
            Response response = await _userService.GetRenterWithStripeInfoAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/exists/{id}
        [HttpGet("exists/{id}")]
        public async Task<ActionResult<bool>> UserExists(Guid id)
        {
            bool exists = await _userService.UserExistsAsync(id);
            return Ok(exists);
        }
    }
}