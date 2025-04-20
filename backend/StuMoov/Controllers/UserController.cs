using Microsoft.AspNetCore.Mvc;
using StuMoov.Models;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel;
using StuMoov.Services.StorageLocationService;
using StuMoov.Services.UserService;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/user
        [HttpGet]
        public ActionResult GetAllUsers()
        {
            Response response = _userService.GetAllUsers();
            return StatusCode(response.Status, response);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public ActionResult GetUserById(Guid id)
        {
            Response response = _userService.GetUserById(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/username/{username}
        [HttpGet("username/{username}")]
        public ActionResult GetUserByUsername(string username)
        {
            Response response = _userService.GetUserByUsername(username);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/email/{email}
        [HttpGet("email/{email}")]
        public ActionResult GetUserByEmail(string email)
        {
            Response response = _userService.GetUserByEmail(email);
            return StatusCode(response.Status, response);
        }

        // GET: api/user/renters
        [HttpGet("renters")]
        public ActionResult GetAllRenters()
        {
            Response response = _userService.GetAllRenters();
            return StatusCode(response.Status, response);
        }

        // GET: api/user/lenders
        [HttpGet("lenders")]
        public ActionResult GetAllLenders()
        {
            Response response = _userService.GetAllLenders();
            return StatusCode(response.Status, response);
        }

        // POST: api/user/register/renter
        [HttpPost("register/renter")]
        public ActionResult RegisterRenter([FromBody] Renter renter)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = _userService.RegisterUser(renter);
            return StatusCode(response.Status, response);
        }

        // POST: api/user/register/lender
        [HttpPost("register/lender")]
        public ActionResult RegisterLender([FromBody] Lender lender)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = _userService.RegisterUser(lender);
            return StatusCode(response.Status, response);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public ActionResult UpdateUser(Guid id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid request data",
                    ModelState
                ));
            }
            Response updateResponse = _userService.UpdateUser(user);
            return StatusCode(updateResponse.Status, updateResponse);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public ActionResult DeleteUser(Guid id)
        {
            Response response = _userService.DeleteUser(id);
            return StatusCode(response.Status, response);
        }
    }
}
