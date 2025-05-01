/**
 * UserService.cs
 *
 * Manages user-related operations for the StuMoov application.
 * Provides functionality to retrieve, register, update, delete, and query users,
 * including specific operations for renters and lenders.
 * Integrates with the UserDao for data access.
 */

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
    /// <summary>
    /// Service responsible for managing user operations, including retrieval, registration,
    /// updates, deletion, and specific queries for renters and lenders.
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// Data access object for user-related database operations.
        /// </summary>
        [Required]
        private readonly UserDao _userDao;

        /// <summary>
        /// Initializes a new instance of the UserService with the required dependency.
        /// </summary>
        /// <param name="userDao">DAO for user operations.</param>
        public UserService(UserDao userDao)
        {
            _userDao = userDao;
        }

        /// <summary>
        /// Retrieves all users from the database.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of users.</returns>
        public async Task<Response> GetAllUsersAsync()
        {
            List<User> users = await _userDao.GetAllUsersAsync();
            return new Response(StatusCodes.Status200OK, "OK", users);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>A Response object with the status, message, and user data, or not found if the user doesn't exist.</returns>
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

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>A Response object with the status, message, and user data, or not found if the user doesn't exist.</returns>
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

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>A Response object with the status, message, and user data, or not found if the user doesn't exist.</returns>
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

        /// <summary>
        /// Registers a new user in the database.
        /// </summary>
        /// <param name="user">The user data to register.</param>
        /// <returns>A Response object with the status, message, and registered user data, or error if registration fails.</returns>
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

        /// <summary>
        /// Updates an existing user's information.
        /// </summary>
        /// <param name="user">The updated user data.</param>
        /// <returns>A Response object with the status, message, and updated user data, or error if update fails.</returns>
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

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="id">The ID of the user to delete.</param>
        /// <returns>A Response object with the status and message, or error if deletion fails.</returns>
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

        /// <summary>
        /// Retrieves all users who are renters.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of renters.</returns>
        public async Task<Response> GetAllRentersAsync()
        {
            List<User> renters = await _userDao.GetAllRentersAsync();
            return new Response(StatusCodes.Status200OK, "OK", renters);
        }

        /// <summary>
        /// Retrieves all users who are lenders.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of lenders.</returns>
        public async Task<Response> GetAllLendersAsync()
        {
            List<User> lenders = await _userDao.GetAllLendersAsync();
            return new Response(StatusCodes.Status200OK, "OK", lenders);
        }

        /// <summary>
        /// Retrieves a renter along with their Stripe information.
        /// </summary>
        /// <param name="id">The ID of the renter.</param>
        /// <returns>A Response object with the status, message, and renter data, or not found if the renter doesn't exist.</returns>
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

        /// <summary>
        /// Retrieves the total count of users in the database.
        /// </summary>
        /// <returns>A Response object with the status, message, and user count.</returns>
        public async Task<Response> GetUserCountAsync()
        {
            int count = await _userDao.CountAsync();

            return new Response(
                StatusCodes.Status200OK,
                "OK",
                count
            );
        }

        /// <summary>
        /// Checks if a user exists by their ID.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        /// <returns>True if the user exists, false otherwise.</returns>
        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _userDao.ExistsAsync(id);
        }
    }
}