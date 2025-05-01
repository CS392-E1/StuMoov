/**
 * StripeCustomerDao.cs
 *
 * Handles data access operations for StripeCustomer entities including retrieval,
 * creation, update, deletion, and updating Stripe-specific details. Uses Entity Framework Core
 * for database interactions.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;

namespace StuMoov.Dao
{
    public class StripeCustomerDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for Stripe customers

        /// <summary>
        /// Initialize the StripeCustomerDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for Stripe customer operations</param>
        public StripeCustomerDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a StripeCustomer by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the StripeCustomer</param>
        /// <returns>The StripeCustomer entity if found; otherwise null</returns>
        public async Task<StripeCustomer?> GetByIdAsync(Guid id)
        {
            return await _dbContext.StripeCustomers
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        /// <summary>
        /// Retrieves a StripeCustomer by the associated user ID.
        /// </summary>
        /// <param name="userId">The GUID of the user</param>
        /// <returns>The StripeCustomer entity if found; otherwise null</returns>
        public async Task<StripeCustomer?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.StripeCustomers
                .FirstOrDefaultAsync(sc => sc.UserId == userId);
        }

        /// <summary>
        /// Retrieves a StripeCustomer by its Stripe Customer ID.
        /// </summary>
        /// <param name="stripeCustomerId">The Stripe customer ID string</param>
        /// <returns>The StripeCustomer entity if found; otherwise null</returns>
        public async Task<StripeCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId)
        {
            return await _dbContext.StripeCustomers
                .FirstOrDefaultAsync(sc => sc.StripeCustomerId == stripeCustomerId);
        }

        /// <summary>
        /// Adds a new StripeCustomer if one does not already exist for the user.
        /// </summary>
        /// <param name="stripeCustomer">The StripeCustomer entity to add</param>
        /// <returns>The saved StripeCustomer with generated fields populated, or null on duplicate or invalid input</returns>
        public async Task<StripeCustomer?> AddAsync(StripeCustomer stripeCustomer)
        {
            if (stripeCustomer == null)
            {
                return null;  // Guard clause for null input
            }

            var existingCustomer = await GetByUserIdAsync(stripeCustomer.UserId);
            if (existingCustomer != null)
            {
                return null;  // Prevent duplicate record for same user
            }

            await _dbContext.StripeCustomers.AddAsync(stripeCustomer);
            await _dbContext.SaveChangesAsync();
            return stripeCustomer;
        }

        /// <summary>
        /// Updates an existing StripeCustomer's details.
        /// </summary>
        /// <param name="stripeCustomer">StripeCustomer model containing updated values</param>
        /// <returns>The updated StripeCustomer entity if found; otherwise null</returns>
        public async Task<StripeCustomer?> UpdateAsync(StripeCustomer stripeCustomer)
        {
            if (stripeCustomer == null)
            {
                return null;  // Guard clause for null input
            }

            var existingCustomer = await _dbContext.StripeCustomers.FindAsync(stripeCustomer.Id);
            if (existingCustomer == null)
            {
                return null;  // No customer to update
            }

            _dbContext.Entry(existingCustomer).CurrentValues.SetValues(stripeCustomer);
            await _dbContext.SaveChangesAsync();
            return stripeCustomer;
        }

        /// <summary>
        /// Deletes a StripeCustomer by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the StripeCustomer to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var stripeCustomer = await _dbContext.StripeCustomers.FindAsync(id);
            if (stripeCustomer == null)
            {
                return false;  // Nothing to delete
            }

            _dbContext.StripeCustomers.Remove(stripeCustomer);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates Stripe-specific details for a customer such as the Stripe customer ID
        /// and optional default payment method.
        /// </summary>
        /// <param name="userId">The GUID of the user whose StripeCustomer to update</param>
        /// <param name="stripeCustomerId">The new Stripe customer ID</param>
        /// <param name="defaultPaymentMethodId">Optional default payment method ID</param>
        /// <returns>The updated StripeCustomer entity if found; otherwise null</returns>
        public async Task<StripeCustomer?> UpdateStripeDetailsAsync(
            Guid userId,
            string stripeCustomerId,
            string? defaultPaymentMethodId = null)
        {
            var customer = await GetByUserIdAsync(userId);
            if (customer == null)
            {
                return null;  // No customer to update
            }

            customer.UpdateStripeInfo(stripeCustomerId, defaultPaymentMethodId);
            await _dbContext.SaveChangesAsync();
            return customer;
        }
    }
}
