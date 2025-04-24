namespace StuMoov.Dao;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;
using StuMoov.Models.UserModel.Enums;

public class StripeConnectAccountDao
{
    [Required]
    private readonly AppDbContext _dbContext;

    public StripeConnectAccountDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get StripeConnectAccount by ID
    public async Task<StripeConnectAccount?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StripeConnectAccounts
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    // Get StripeConnectAccount by User ID
    public async Task<StripeConnectAccount?> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.StripeConnectAccounts
            .FirstOrDefaultAsync(sc => sc.UserId == userId);
    }

    // Get StripeConnectAccount by Stripe Connect Account ID
    public async Task<StripeConnectAccount?> GetByStripeConnectAccountIdAsync(string stripeConnectAccountId)
    {
        return await _dbContext.StripeConnectAccounts
            .FirstOrDefaultAsync(sc => sc.StripeConnectAccountId == stripeConnectAccountId);
    }

    // Add new StripeConnectAccount
    public async Task<StripeConnectAccount?> AddAsync(StripeConnectAccount stripeConnectAccount)
    {
        if (stripeConnectAccount == null)
        {
            return null;
        }

        // Check if a record already exists for this user
        StripeConnectAccount? existingAccount = await GetByUserIdAsync(stripeConnectAccount.UserId);
        if (existingAccount != null)
        {
            return null;
        }

        await _dbContext.StripeConnectAccounts.AddAsync(stripeConnectAccount);
        await _dbContext.SaveChangesAsync();

        return stripeConnectAccount;
    }

    // Update existing StripeConnectAccount
    public async Task<StripeConnectAccount?> UpdateAsync(StripeConnectAccount stripeConnectAccount)
    {
        if (stripeConnectAccount == null)
        {
            return null;
        }

        StripeConnectAccount? existingAccount = await _dbContext.StripeConnectAccounts.FindAsync(stripeConnectAccount.Id);
        if (existingAccount == null)
        {
            return null;
        }

        _dbContext.Entry(existingAccount).CurrentValues.SetValues(stripeConnectAccount);
        await _dbContext.SaveChangesAsync();

        return stripeConnectAccount;
    }

    // Delete StripeConnectAccount
    public async Task<bool> DeleteAsync(Guid id)
    {
        StripeConnectAccount? stripeConnectAccount = await _dbContext.StripeConnectAccounts.FindAsync(id);
        if (stripeConnectAccount == null)
        {
            return false;
        }

        _dbContext.StripeConnectAccounts.Remove(stripeConnectAccount);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Update status of a StripeConnectAccount
    public async Task<StripeConnectAccount?> UpdateStatusAsync(Guid id, StripeConnectAccountStatus status, bool payoutsEnabled)
    {
        StripeConnectAccount? account = await GetByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        StripeConnectAccount? entity = await _dbContext.StripeConnectAccounts.FindAsync(id);
        if (entity == null)
        {
            return null;
        }

        // Use the new UpdateStatus method
        entity.UpdateStatus(status, payoutsEnabled);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    // Update account link URL
    public async Task<StripeConnectAccount?> UpdateAccountLinkUrlAsync(Guid id, string accountLinkUrl)
    {
        StripeConnectAccount? account = await GetByIdAsync(id);
        if (account == null)
        {
            return null;
        }

        StripeConnectAccount? entity = await _dbContext.StripeConnectAccounts.FindAsync(id);
        if (entity == null)
        {
            return null;
        }

        // Use the new UpdateAccountLinkUrl method
        entity.UpdateAccountLinkUrl(accountLinkUrl);
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}