namespace StuMoov.Dao;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;

public class StripeCustomerDao
{
    [Required]
    private readonly AppDbContext _dbContext;

    public StripeCustomerDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get StripeCustomer by ID
    public async Task<StripeCustomer?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StripeCustomers
            .FirstOrDefaultAsync(sc => sc.Id == id);
    }

    // Get StripeCustomer by User ID
    public async Task<StripeCustomer?> GetByUserIdAsync(Guid userId)
    {
        return await _dbContext.StripeCustomers
            .FirstOrDefaultAsync(sc => sc.UserId == userId);
    }

    // Get StripeCustomer by Stripe Customer ID
    public async Task<StripeCustomer?> GetByStripeCustomerIdAsync(string stripeCustomerId)
    {
        return await _dbContext.StripeCustomers
            .FirstOrDefaultAsync(sc => sc.StripeCustomerId == stripeCustomerId);
    }

    // Add new StripeCustomer
    public async Task<StripeCustomer?> AddAsync(StripeCustomer stripeCustomer)
    {
        if (stripeCustomer == null)
        {
            return null;
        }

        // Check if a record already exists for this user
        StripeCustomer? existingCustomer = await GetByUserIdAsync(stripeCustomer.UserId);
        if (existingCustomer != null)
        {
            return null;
        }

        await _dbContext.StripeCustomers.AddAsync(stripeCustomer);
        await _dbContext.SaveChangesAsync();

        return stripeCustomer;
    }

    // Update existing StripeCustomer
    public async Task<StripeCustomer?> UpdateAsync(StripeCustomer stripeCustomer)
    {
        if (stripeCustomer == null)
        {
            return null;
        }

        StripeCustomer? existingCustomer = await _dbContext.StripeCustomers.FindAsync(stripeCustomer.Id);
        if (existingCustomer == null)
        {
            return null;
        }

        _dbContext.Entry(existingCustomer).CurrentValues.SetValues(stripeCustomer);
        await _dbContext.SaveChangesAsync();

        return stripeCustomer;
    }

    // Delete StripeCustomer
    public async Task<bool> DeleteAsync(Guid id)
    {
        StripeCustomer? stripeCustomer = await _dbContext.StripeCustomers.FindAsync(id);
        if (stripeCustomer == null)
        {
            return false;
        }

        _dbContext.StripeCustomers.Remove(stripeCustomer);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Update Stripe details for a customer
    public async Task<StripeCustomer?> UpdateStripeDetailsAsync(Guid userId, string stripeCustomerId, string? defaultPaymentMethodId = null)
    {
        StripeCustomer? customer = await GetByUserIdAsync(userId);
        if (customer == null)
        {
            return null;
        }

        customer.UpdateStripeInfo(stripeCustomerId, defaultPaymentMethodId);
        await _dbContext.SaveChangesAsync();

        return customer;
    }
}