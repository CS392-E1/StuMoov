/**
 * BookingDao.cs
 * 
 * Handles data access operations for Booking entities including retrieval,
 * creation, update, and deletion. Uses Entity Framework Core for database interactions.
 */

namespace StuMoov.Dao
{
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
        private readonly AppDbContext _dbContext;  // EF Core database context

        /// <summary>
        /// Initialize the BookingDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for bookings</param>
        public BookingDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves all bookings from the database, including related payment,
        /// renter, and storage location entities.
        /// </summary>
        /// <returns>List of all Booking entities</returns>
        public async Task<List<Booking>> GetAllAsync()
        {
            return await _dbContext.Bookings
                .Include(b => b.Payment)
                .Include(b => b.Renter)
                .Include(b => b.StorageLocation)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a booking by its unique identifier, including nested
        /// Stripe and related entities.
        /// </summary>
        /// <param name="id">The unique identifier of the booking</param>
        /// <returns>The Booking entity if found; otherwise null</returns>
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

        /// <summary>
        /// Retrieves all bookings for a specific renter.
        /// </summary>
        /// <param name="renterId">The unique identifier of the renter</param>
        /// <returns>List of Booking entities for the renter</returns>
        public async Task<List<Booking>> GetByRenterIdAsync(Guid renterId)
        {
            return await _dbContext.Bookings
                .Where(b => b.RenterId == renterId)
                .Include(b => b.Payment)
                .Include(b => b.StorageLocation)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all bookings for a specific storage location.
        /// </summary>
        /// <param name="storageLocationId">The unique identifier of the storage location</param>
        /// <returns>List of Booking entities for the storage location</returns>
        public async Task<List<Booking>> GetByStorageLocationIdAsync(Guid storageLocationId)
        {
            return await _dbContext.Bookings
                .Where(b => b.StorageLocationId == storageLocationId)
                .Include(b => b.Payment)
                .Include(b => b.Renter)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all bookings with a specific status.
        /// </summary>
        /// <param name="status">The BookingStatus to filter by</param>
        /// <returns>List of Booking entities matching the status</returns>
        public async Task<List<Booking>> GetByStatusAsync(BookingStatus status)
        {
            return await _dbContext.Bookings
                .Where(b => b.Status == status)
                .Include(b => b.Payment)
                .Include(b => b.Renter)
                .Include(b => b.StorageLocation)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new booking record in the database.
        /// </summary>
        /// <param name="booking">The Booking entity to create</param>
        /// <returns>The GUID of the newly created booking</returns>
        public async Task<Guid> CreateAsync(Booking booking)
        {
            await _dbContext.Bookings.AddAsync(booking);
            await _dbContext.SaveChangesAsync();

            return booking.Id;
        }

        /// <summary>
        /// Updates the status of an existing booking.
        /// </summary>
        /// <param name="id">The unique identifier of the booking to update</param>
        /// <param name="newStatus">The new BookingStatus value</param>
        /// <returns>True if the update succeeded; otherwise false</returns>
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

        /// <summary>
        /// Updates all editable fields of an existing booking, except
        /// related navigation properties.
        /// </summary>
        /// <param name="bookingUpdate">Booking model containing updated values</param>
        /// <returns>True if the update succeeded; otherwise false</returns>
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

        /// <summary>
        /// Deletes a booking by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the booking to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
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

        /// <summary>
        /// Retrieves all active bookings (i.e., not cancelled).
        /// </summary>
        /// <returns>List of active Booking entities</returns>
        public async Task<List<Booking>> GetActiveAsync()
        {
            return await _dbContext.Bookings
                .Where(b => b.Status != BookingStatus.CANCELLED)
                .Include(b => b.Payment)
                .Include(b => b.Renter)
                .Include(b => b.StorageLocation)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves bookings that overlap with a given date range.
        /// </summary>
        /// <param name="startDate">Start of the date range</param>
        /// <param name="endDate">End of the date range</param>
        /// <returns>List of Booking entities in the date range</returns>
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

        /// <summary>
        /// Checks if a storage location is available for booking within
        /// a specified date range (excluding cancelled bookings).
        /// </summary>
        /// <param name="storageLocationId">The storage location's GUID</param>
        /// <param name="startDate">Start of desired booking window</param>
        /// <param name="endDate">End of desired booking window</param>
        /// <returns>True if available; otherwise false</returns>
        public async Task<bool> IsStorageLocationAvailableAsync(
            Guid storageLocationId,
            DateTime startDate,
            DateTime endDate)
        {
            var conflicting = await _dbContext.Bookings
                .Where(b =>
                    b.StorageLocationId == storageLocationId &&
                    b.Status != BookingStatus.CANCELLED &&
                    ((b.StartDate >= startDate && b.StartDate < endDate) ||
                     (b.EndDate > startDate && b.EndDate <= endDate) ||
                     (b.StartDate <= startDate && b.EndDate >= endDate)))
                .AnyAsync();

            return !conflicting;
        }

        /// <summary>
        /// Gets the total number of bookings in the system.
        /// </summary>
        /// <returns>Total count of bookings</returns>
        public async Task<int> CountAsync()
        {
            return await _dbContext.Bookings.CountAsync();
        }

        /// <summary>
        /// Checks whether a booking exists by its unique identifier.
        /// </summary>
        /// <param name="id">The booking's GUID</param>
        /// <returns>True if the booking exists; otherwise false</returns>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbContext.Bookings.AnyAsync(b => b.Id == id);
        }

        /// <summary>
        /// Retrieves all bookings associated with a specific payment.
        /// </summary>
        /// <param name="paymentId">The unique identifier of the payment</param>
        /// <returns>List of Booking entities linked to the payment</returns>
        public async Task<List<Booking>> GetByPaymentIdAsync(Guid paymentId)
        {
            return await _dbContext.Bookings
                .Where(b => b.PaymentId == paymentId)
                .Include(b => b.Renter)
                .Include(b => b.StorageLocation)
                .ToListAsync();
        }
    }
}