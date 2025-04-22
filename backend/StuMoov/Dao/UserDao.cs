namespace StuMoov.Dao;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.UserModel;

public class UserDao
{
    [Required]
    private readonly AppDbContext _dbContext;

    public UserDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Get user by ID
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // Get user by username
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.DisplayName.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    // Get user by email
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
               .Where(u => u.Email.ToLower() == email.ToLower())   //Don't Change to Equals
               .FirstOrDefaultAsync();
    }

    // Add new user
    public async Task<User?> AddUserAsync(User user)
    {
        if (user == null)
        {
            return null;
        }

        // Check if email already exists
        var existingUser = await GetUserByEmailAsync(user.Email);
        if (existingUser != null)
        {
            return null;
        }

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }

    // Update existing user
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

    // Delete user
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

    // Get all users
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }

    // Get all renters
    public async Task<List<User>> GetAllRentersAsync()
    {
        return await _dbContext.Users
            .Where(u => u is Renter)
            .ToListAsync();
    }

    // Get all lenders
    public async Task<List<User>> GetAllLendersAsync()
    {
        return await _dbContext.Users
            .Where(u => u is Lender)
            .ToListAsync();
    }

    // Check if a user exists by ID
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == id);
    }

    // Count total number of users
    public async Task<int> CountAsync()
    {
        return await _dbContext.Users.CountAsync();
    }

    // Get a renter with their stripe customer info
    public async Task<Renter?> GetRenterWithStripeInfoAsync(Guid id)
    {
        return await _dbContext.Users
            .OfType<Renter>()
            .Include(r => r.StripeCustomerInfo)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
