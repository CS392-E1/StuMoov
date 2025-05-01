/**
 * UserDao.cs
 *
 * Handles data access operations for User entities including retrieval,
 * creation, update, deletion, and specialized queries for renters and lenders.
 * Uses Entity Framework Core for database interactions.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;

namespace StuMoov.Dao
{
    public class UserDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for users

        /// <summary>
        /// Initialize the UserDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for user operations</param>
        public UserDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the user</param>
        /// <returns>The User entity if found; otherwise null</returns>
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Retrieves a user by their display name (case-insensitive).
        /// </summary>
        /// <param name="username">The display name of the user</param>
        /// <returns>The User entity if found; otherwise null</returns>
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.DisplayName.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user</param>
        /// <returns>The User entity if found; otherwise null</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbContext.Users
                   .Where(u => u.Email.ToLower() == email.ToLower())  // don't change to Equals for case-insensitive match
                   .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Adds a new user if the email does not already exist.
        /// </summary>
        /// <param name="user">The User entity to add</param>
        /// <returns>The created User entity, or null if input is invalid or email exists</returns>
        public async Task<User?> AddUserAsync(User user)
        {
            if (user == null)
            {
                return null;  // guard clause for null input
            }

            var existingUser = await GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                return null;  // prevent duplicate email
            }

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Updates an existing user's data.
        /// </summary>
        /// <param name="user">User entity containing updated values</param>
        /// <returns>The updated User entity if found; otherwise null</returns>
        public async Task<User?> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                return null;
            }

            var existingUser = await _dbContext.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                return null;
            }

            _dbContext.Entry(existingUser).CurrentValues.SetValues(user);
            await _dbContext.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Deletes a user by their unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the user to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Retrieves all users.
        /// </summary>
        /// <returns>List of all User entities</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        /// <summary>
        /// Retrieves all users who are renters.
        /// </summary>
        /// <returns>List of Renter entities</returns>
        public async Task<List<User>> GetAllRentersAsync()
        {
            return await _dbContext.Users
                .Where(u => u is Renter)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all users who are lenders.
        /// </summary>
        /// <returns>List of Lender entities</returns>
        public async Task<List<User>> GetAllLendersAsync()
        {
            return await _dbContext.Users
                .Where(u => u is Lender)
                .ToListAsync();
        }

        /// <summary>
        /// Checks if a user exists by their unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the user</param>
        /// <returns>True if the user exists; otherwise false</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Users.AnyAsync(u => u.Id == id);
        }

        /// <summary>
        /// Counts the total number of users in the database.
        /// </summary>
        /// <returns>Total count of users</returns>
        public async Task<int> CountAsync()
        {
            return await _dbContext.Users.CountAsync();
        }

        /// <summary>
        /// Retrieves a Renter with their associated Stripe customer info.
        /// </summary>
        /// <param name="id">The GUID of the renter</param>
        /// <returns>The Renter entity if found; otherwise null</returns>
        public async Task<Renter?> GetRenterWithStripeInfoAsync(Guid id)
        {
            return await _dbContext.Users
                .OfType<Renter>()
                .Include(r => r.StripeCustomerInfo)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
