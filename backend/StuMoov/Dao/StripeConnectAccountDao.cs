/**
 * StripeConnectAccountDao.cs
 *
 * Handles data access operations for StripeConnectAccount entities including retrieval,
 * creation, update, deletion, and status/account link URL updates. Uses Entity Framework Core
 * for database interactions.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Dao
{
    public class StripeConnectAccountDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for Stripe Connect accounts

        /// <summary>
        /// Initialize the StripeConnectAccountDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for Stripe Connect account operations</param>
        public StripeConnectAccountDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a StripeConnectAccount by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the StripeConnectAccount</param>
        /// <returns>The StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> GetByIdAsync(Guid id)
        {
            return await _dbContext.StripeConnectAccounts
                .FirstOrDefaultAsync(sc => sc.Id == id);
        }

        /// <summary>
        /// Retrieves a StripeConnectAccount by the associated user ID.
        /// </summary>
        /// <param name="userId">The GUID of the user</param>
        /// <returns>The StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.StripeConnectAccounts
                .FirstOrDefaultAsync(sc => sc.UserId == userId);
        }

        /// <summary>
        /// Retrieves a StripeConnectAccount by its Stripe Connect account ID.
        /// </summary>
        /// <param name="stripeConnectAccountId">The Stripe Connect account ID string</param>
        /// <returns>The StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> GetByStripeConnectAccountIdAsync(string stripeConnectAccountId)
        {
            return await _dbContext.StripeConnectAccounts
                .FirstOrDefaultAsync(sc => sc.StripeConnectAccountId == stripeConnectAccountId);
        }

        /// <summary>
        /// Adds a new StripeConnectAccount if one does not already exist for the user.
        /// </summary>
        /// <param name="stripeConnectAccount">The StripeConnectAccount entity to add</param>
        /// <returns>The saved StripeConnectAccount with generated fields populated, or null on duplicate or invalid input</returns>
        public async Task<StripeConnectAccount?> AddAsync(StripeConnectAccount stripeConnectAccount)
        {
            if (stripeConnectAccount == null)
            {
                return null;  // Guard clause for null input
            }

            var existingAccount = await GetByUserIdAsync(stripeConnectAccount.UserId);
            if (existingAccount != null)
            {
                return null;  // Prevent duplicate record for same user
            }

            await _dbContext.StripeConnectAccounts.AddAsync(stripeConnectAccount);
            await _dbContext.SaveChangesAsync();
            return stripeConnectAccount;
        }

        /// <summary>
        /// Updates an existing StripeConnectAccount's details.
        /// </summary>
        /// <param name="stripeConnectAccount">StripeConnectAccount model containing updated values</param>
        /// <returns>The updated StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> UpdateAsync(StripeConnectAccount stripeConnectAccount)
        {
            if (stripeConnectAccount == null)
            {
                return null;  // Guard clause for null input
            }

            var existingAccount = await _dbContext.StripeConnectAccounts.FindAsync(stripeConnectAccount.Id);
            if (existingAccount == null)
            {
                return null;  // No account to update
            }

            _dbContext.Entry(existingAccount).CurrentValues.SetValues(stripeConnectAccount);
            await _dbContext.SaveChangesAsync();
            return stripeConnectAccount;
        }

        /// <summary>
        /// Deletes a StripeConnectAccount by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the StripeConnectAccount to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var stripeConnectAccount = await _dbContext.StripeConnectAccounts.FindAsync(id);
            if (stripeConnectAccount == null)
            {
                return false;  // Nothing to delete
            }

            _dbContext.StripeConnectAccounts.Remove(stripeConnectAccount);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Updates the status and payoutsEnabled flag of an existing StripeConnectAccount.
        /// </summary>
        /// <param name="id">The GUID of the StripeConnectAccount to update</param>
        /// <param name="status">The new StripeConnectAccountStatus</param>
        /// <param name="payoutsEnabled">Flag indicating if payouts are enabled</param>
        /// <returns>The updated StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> UpdateStatusAsync(Guid id, StripeConnectAccountStatus status, bool payoutsEnabled)
        {
            var entity = await _dbContext.StripeConnectAccounts.FindAsync(id);
            if (entity == null)
            {
                return null;  // No account to update
            }

            entity.UpdateStatus(status, payoutsEnabled);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Updates the account link URL for an existing StripeConnectAccount.
        /// </summary>
        /// <param name="id">The GUID of the StripeConnectAccount to update</param>
        /// <param name="accountLinkUrl">The new account link URL</param>
        /// <returns>The updated StripeConnectAccount entity if found; otherwise null</returns>
        public async Task<StripeConnectAccount?> UpdateAccountLinkUrlAsync(Guid id, string accountLinkUrl)
        {
            var entity = await _dbContext.StripeConnectAccounts.FindAsync(id);
            if (entity == null)
            {
                return null;  // No account to update
            }

            entity.UpdateAccountLinkUrl(accountLinkUrl);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}
