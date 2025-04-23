using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        public async Task<Response> GetAllUsersAsync()
        {
            List<User> users = await _userDao.GetAllUsersAsync();
            return new Response(StatusCodes.Status200OK, "OK", users);
        }

        // Get user by ID
        public async Task<Response> GetUserByIdAsync(Guid id)
        {
            User? user = await _userDao.GetUserByIdAsync(id);

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
        public async Task<Response> GetUserByUsernameAsync(string username)
        {
            User? user = await _userDao.GetUserByUsernameAsync(username);

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
        public async Task<Response> GetUserByEmailAsync(string email)
        {
            User? user = await _userDao.GetUserByEmailAsync(email);

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
        public async Task<Response> RegisterUserAsync(User user)
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

            // Add user to database
            User? registeredUser = await _userDao.AddUserAsync(user);

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
        public async Task<Response> UpdateUserAsync(User user)
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
            User? existingUser = await _userDao.GetUserByIdAsync(user.Id);
            if (existingUser == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            // Check if new email conflicts with another user
            User? userWithSameEmail = await _userDao.GetUserByEmailAsync(user.Email);
            if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
            {
                return new Response(
                    StatusCodes.Status409Conflict,
                    "Email already exists",
                    null
                );
            }

            // Update user
            User? updatedUser = await _userDao.UpdateUserAsync(user);

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
        public async Task<Response> DeleteUserAsync(Guid id)
        {
            // Check if user exists
            User? existingUser = await _userDao.GetUserByIdAsync(id);
            if (existingUser == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "User not found",
                    null
                );
            }

            // Delete user
            bool deleted = await _userDao.DeleteUserAsync(id);

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
        public async Task<Response> GetAllRentersAsync()
        {
            List<User> renters = await _userDao.GetAllRentersAsync();
            return new Response(StatusCodes.Status200OK, "OK", renters);
        }

        // Get all lenders
        public async Task<Response> GetAllLendersAsync()
        {
            List<User> lenders = await _userDao.GetAllLendersAsync();
            return new Response(StatusCodes.Status200OK, "OK", lenders);
        }

        // Get renter with stripe information
        public async Task<Response> GetRenterWithStripeInfoAsync(Guid id)
        {
            Renter? renter = await _userDao.GetRenterWithStripeInfoAsync(id);

            if (renter == null)
            {
                return new Response(
                    StatusCodes.Status404NotFound,
                    "Renter not found",
                    null
                );
            }

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                new List<User> { renter }
            );
        }

        // Get user count
        public async Task<Response> GetUserCountAsync()
        {
            int count = await _userDao.CountAsync();

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                count
            );
        }

        // Check if user exists
        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _userDao.ExistsAsync(id);
        }
    }
}