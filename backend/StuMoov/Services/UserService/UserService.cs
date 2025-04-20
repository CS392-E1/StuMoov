using System.ComponentModel.DataAnnotations;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.UserModel;

namespace StuMoov.Services.UserService
{
    public class UserService
    {
        [Required]
        private readonly UserDao _userDao;

        public UserService(UserDao userDao)
        {
            _userDao = userDao;
        }

        // Get all users
        public Response GetAllUsers()
        {
            List<User> users = _userDao.GetAllUsers();
            return new Response(StatusCodes.Status200OK, "OK", users);
        }

        // Get user by ID
        public Response GetUserById(Guid id)
        {
            User? user = _userDao.GetUserById(id);

            if (user == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                new List<User> { user }
            );
        }

        // Get user by username
        public Response GetUserByUsername(string username)
        {
            User? user = _userDao.GetUserByUsername(username);

            if (user == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                new List<User> { user }
            );
        }

        // Get user by email
        public Response GetUserByEmail(string email)
        {
            User? user = _userDao.GetUserByEmail(email);

            if (user == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                new List<User> { user }
            );
        }

        // Register a new user
        public Response RegisterUser(User user)
        {
            // Check if user is null
            if (user == null)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid user data",
                    null
                );
            }

            // Check if username or email already exists
            if (_userDao.GetUserByUsername(user.Username) != null)
            {
                return new Response(
                    StatusCodes.Status409Conflict,
                    "Username already exists",
                    null
                );
            }

            if (_userDao.GetUserByEmail(user.Email) != null)
            {
                return new Response(
                    StatusCodes.Status409Conflict,
                    "Email already exists",
                    null
                );
            }

            // Add user to database
            User? registeredUser = _userDao.AddUser(user);

            if (registeredUser == null)
            {
                return new Response(
                    StatusCodes.Status500InternalServerError,
                    "Failed to register user",
                    null
                );
            }

            return new Response(
                StatusCodes.Status201Created,
                "User registered successfully",
                new List<User> { registeredUser }
            );
        }

        // Update existing user
        public Response UpdateUser(User user)
        {
            // Check if user is null
            if (user == null)
            {
                return new Response(
                    StatusCodes.Status400BadRequest,
                    "Invalid user data",
                    null
                );
            }

            // Check if user exists
            User? existingUser = _userDao.GetUserById(user.Id);
            if (existingUser == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            // Check if new username conflicts with another user
            User? userWithSameUsername = _userDao.GetUserByUsername(user.Username);
            if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
            {
                return new Response(
                    StatusCodes.Status409Conflict,
                    "Username already exists",
                    null
                );
            }

            // Check if new email conflicts with another user
            User? userWithSameEmail = _userDao.GetUserByEmail(user.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
            {
                return new Response(
                    StatusCodes.Status409Conflict,
                    "Email already exists",
                    null
                );
            }

            // Update user
            User? updatedUser = _userDao.UpdateUser(user);

            if (updatedUser == null)
            {
                return new Response(
                    StatusCodes.Status500InternalServerError,
                    "Failed to update user",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "User updated successfully",
                new List<User> { updatedUser }
            );
        }


        // Delete user
        public Response DeleteUser(Guid id)
        {
            // Check if user exists
            User? existingUser = _userDao.GetUserById(id);
            if (existingUser == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            // Delete user
            bool deleted = _userDao.DeleteUser(id);

            if (!deleted)
            {
                return new Response(
                    StatusCodes.Status500InternalServerError,
                    "Failed to delete user",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "User deleted successfully",
                null
            );
        }

        // Get all renters
        public Response GetAllRenters()
        {
            List<User> renters = _userDao.GetAllRenters();
            return new Response(StatusCodes.Status200OK, "OK", renters);
        }

        // Get all lenders
        public Response GetAllLenders()
        {
            List<User> lenders = _userDao.GetAllLenders();
            return new Response(StatusCodes.Status200OK, "OK", lenders);
        }

    }
}
