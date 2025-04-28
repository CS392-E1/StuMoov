namespace StuMoov.Dao;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.BookingModel;

public class BookingDao
{
    [Required]
    private readonly AppDbContext _dbContext;

    public BookingDao(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Method to retrieve all bookings
    public async Task<List<Booking>> GetAllAsync()
    {
        return await _dbContext.Bookings
            .Include(b => b.Payment)
            .Include(b => b.Renter)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }

    // Get a booking by ID
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Bookings
            .Include(b => b.Payment)
            .Include(b => b.Renter)
                .ThenInclude(r => r.StripeCustomerInfo)
            .Include(b => b.StorageLocation)
                .ThenInclude(sl => sl.Lender)
                    .ThenInclude(l => l.StripeConnectInfo)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    // Get bookings by renter ID
    public async Task<List<Booking>> GetByRenterIdAsync(Guid renterId)
    {
        return await _dbContext.Bookings
            .Where(b => b.RenterId == renterId)
            .Include(b => b.Payment)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }

    // Get bookings by storage location ID
    public async Task<List<Booking>> GetByStorageLocationIdAsync(Guid storageLocationId)
    {
        return await _dbContext.Bookings
            .Where(b => b.StorageLocationId == storageLocationId)
            .Include(b => b.Payment)
            .Include(b => b.Renter)
            .ToListAsync();
    }

    // Get bookings by status
    public async Task<List<Booking>> GetByStatusAsync(BookingStatus status)
    {
        return await _dbContext.Bookings
            .Where(b => b.Status == status)
            .Include(b => b.Payment)
            .Include(b => b.Renter)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }

    // Create a new booking
    public async Task<Guid> CreateAsync(Booking booking)
    {
        await _dbContext.Bookings.AddAsync(booking);
        await _dbContext.SaveChangesAsync();

        return booking.Id;
    }

    // Update booking status
    public async Task<bool> UpdateStatusAsync(Guid id, BookingStatus newStatus)
    {
        var booking = await _dbContext.Bookings.FindAsync(id);
        if (booking == null)
        {
            return false;
        }

        booking.Status = newStatus;
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(Booking bookingUpdate)
    {
        var booking = await _dbContext.Bookings.FindAsync(bookingUpdate.Id);
        if (booking == null)
        {
            return false;
        }

        _dbContext.Entry(booking).CurrentValues.SetValues(bookingUpdate);
        _dbContext.Entry(booking).Reference(b => b.Payment).IsModified = false;
        _dbContext.Entry(booking).Reference(b => b.Renter).IsModified = false;
        _dbContext.Entry(booking).Reference(b => b.StorageLocation).IsModified = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Delete a booking
    public async Task<bool> DeleteAsync(Guid id)
    {
        var booking = await _dbContext.Bookings.FindAsync(id);
        if (booking == null)
        {
            return false;
        }

        _dbContext.Bookings.Remove(booking);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // Get active bookings (not cancelled)
    public async Task<List<Booking>> GetActiveAsync()
    {
        return await _dbContext.Bookings
            .Where(b => b.Status != BookingStatus.CANCELLED)
            .Include(b => b.Payment)
            .Include(b => b.Renter)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }

    // Get bookings for a date range
    public async Task<List<Booking>> GetForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _dbContext.Bookings
            .Where(b =>
                (b.StartDate >= startDate && b.StartDate <= endDate) ||
                (b.EndDate >= startDate && b.EndDate <= endDate) ||
                (b.StartDate <= startDate && b.EndDate >= endDate))
            .Include(b => b.Payment)
            .Include(b => b.Renter)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }

    // Check if a storage location is available for a date range
    public async Task<bool> IsStorageLocationAvailableAsync(Guid storageLocationId, DateTime startDate, DateTime endDate)
    {
        var conflictingBookings = await _dbContext.Bookings
            .Where(b =>
                b.StorageLocationId == storageLocationId &&
                b.Status != BookingStatus.CANCELLED &&
                ((b.StartDate >= startDate && b.StartDate < endDate) ||
                 (b.EndDate > startDate && b.EndDate <= endDate) ||
                 (b.StartDate <= startDate && b.EndDate >= endDate)))
            .AnyAsync();

        return !conflictingBookings;
    }

    // Count total number of bookings
    public async Task<int> CountAsync()
    {
        return await _dbContext.Bookings.CountAsync();
    }

    // Check if a booking exists by ID
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbContext.Bookings.AnyAsync(b => b.Id == id);
    }

    // Get bookings by payment ID
    public async Task<List<Booking>> GetByPaymentIdAsync(Guid paymentId)
    {
        return await _dbContext.Bookings
            .Where(b => b.PaymentId == paymentId)
            .Include(b => b.Renter)
            .Include(b => b.StorageLocation)
            .ToListAsync();
    }
}
