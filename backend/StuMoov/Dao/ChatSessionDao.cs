/**
 * ChatSessionDao.cs
 * 
 * Handles data access operations for ChatSession entities including retrieval,
 * creation, update, and deletion. Uses Entity Framework Core for database interactions.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.ChatModel;

namespace StuMoov.Dao
{
    public class ChatSessionDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for chat sessions

        /// <summary>
        /// Initialize the ChatSessionDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for chat sessions</param>
        public ChatSessionDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a chat session by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the chat session</param>
        /// <returns>The ChatSession entity if found; otherwise null</returns>
        public async Task<ChatSession?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChatSessions
                .Include(cs => cs.Renter)   // Include renter details
                .Include(cs => cs.Lender)   // Include lender details
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        /// <summary>
        /// Retrieves all chat sessions for a given renter.
        /// </summary>
        /// <param name="renterId">The GUID of the renter</param>
        /// <returns>List of ChatSession entities where the renter is involved</returns>
        public async Task<List<ChatSession>> GetByRenterIdAsync(Guid renterId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.RenterId == renterId)  // Filter by renter ID
                .Include(cs => cs.Lender)               // Include lender info
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all chat sessions for a given lender.
        /// </summary>
        /// <param name="lenderId">The GUID of the lender</param>
        /// <returns>List of ChatSession entities where the lender is involved</returns>
        public async Task<List<ChatSession>> GetByLenderIdAsync(Guid lenderId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.LenderId == lenderId)  // Filter by lender ID
                .Include(cs => cs.Renter)               // Include renter info
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a chat session associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">The GUID of the booking</param>
        /// <returns>The ChatSession entity if found; otherwise null</returns>
        public async Task<ChatSession?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.ChatSessions
                .Include(cs => cs.Renter)   // Include renter details
                .Include(cs => cs.Lender)   // Include lender details
                .FirstOrDefaultAsync(cs => cs.BookingId == bookingId);
        }

        /// <summary>
        /// Adds a new chat session to the database.
        /// </summary>
        /// <param name="chatSession">The ChatSession entity to add</param>
        /// <returns>The saved ChatSession with generated fields populated, or null if input is null</returns>
        public async Task<ChatSession?> AddAsync(ChatSession chatSession)
        {
            if (chatSession == null)
            {
                return null;                       // Guard clause for null input
            }

            await _dbContext.ChatSessions.AddAsync(chatSession);
            await _dbContext.SaveChangesAsync();   // Persist changes

            return chatSession;
        }

        /// <summary>
        /// Updates an existing chat session's details.
        /// </summary>
        /// <param name="chatSession">ChatSession model containing updated values</param>
        /// <returns>The updated ChatSession if found; otherwise null</returns>
        public async Task<ChatSession?> UpdateAsync(ChatSession chatSession)
        {
            if (chatSession == null)
            {
                return null;                       // Guard clause for null input
            }

            var existingSession = await _dbContext.ChatSessions.FindAsync(chatSession.Id);
            if (existingSession == null)
            {
                return null;                       // No session to update
            }

            _dbContext.Entry(existingSession).CurrentValues.SetValues(chatSession);
            await _dbContext.SaveChangesAsync();   // Persist updates

            return existingSession;
        }

        /// <summary>
        /// Deletes a chat session by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the chat session to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var chatSession = await _dbContext.ChatSessions.FindAsync(id);
            if (chatSession == null)
            {
                return false;                      // Nothing to delete
            }

            _dbContext.ChatSessions.Remove(chatSession);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all chat sessions involving a specific user, either as renter or lender.
        /// </summary>
        /// <param name="userId">The GUID of the user</param>
        /// <returns>List of ChatSession entities involving the user</returns>
        public async Task<List<ChatSession>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.RenterId == userId || cs.LenderId == userId)  // Filter by either role
                .Include(cs => cs.Renter)
                .Include(cs => cs.Lender)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a chat session by participants (renter, lender) and a storage listing.
        /// </summary>
        /// <param name="renterId">The GUID of the renter</param>
        /// <param name="lenderId">The GUID of the lender</param>
        /// <param name="storageLocationId">The GUID of the storage location</param>
        /// <returns>The ChatSession entity if found; otherwise null</returns>
        public async Task<ChatSession?> GetByParticipantsAndListingAsync(
            Guid renterId,
            Guid lenderId,
            Guid storageLocationId)
        {
            return await _dbContext.ChatSessions
                .Include(cs => cs.Renter)
                .Include(cs => cs.Lender)
                .Include(cs => cs.StorageLocation)
                .FirstOrDefaultAsync(cs =>
                    cs.RenterId == renterId &&
                    cs.LenderId == lenderId &&
                    cs.StorageLocationId == storageLocationId);
        }
    }
}