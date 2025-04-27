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
        private readonly AppDbContext _dbContext;

        public ChatSessionDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get ChatSession by ID
        public async Task<ChatSession?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChatSessions
                .Include(cs => cs.Renter)
                .Include(cs => cs.Lender)
                .FirstOrDefaultAsync(cs => cs.Id == id);
        }

        // Get ChatSessions by Renter ID
        public async Task<List<ChatSession>> GetByRenterIdAsync(Guid renterId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.RenterId == renterId)
                .Include(cs => cs.Lender)
                .ToListAsync();
        }

        // Get ChatSessions by Lender ID
        public async Task<List<ChatSession>> GetByLenderIdAsync(Guid lenderId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.LenderId == lenderId)
                .Include(cs => cs.Renter)
                .ToListAsync();
        }

        // Get ChatSession by Booking ID
        public async Task<ChatSession?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.ChatSessions
               .Include(cs => cs.Renter)
               .Include(cs => cs.Lender)
               .FirstOrDefaultAsync(cs => cs.BookingId == bookingId);
        }

        // Add new ChatSession
        public async Task<ChatSession?> AddAsync(ChatSession chatSession)
        {
            if (chatSession == null)
            {
                return null;
            }

            await _dbContext.ChatSessions.AddAsync(chatSession);
            await _dbContext.SaveChangesAsync();

            return chatSession;
        }

        // Update existing ChatSession
        public async Task<ChatSession?> UpdateAsync(ChatSession chatSession)
        {
            if (chatSession == null)
            {
                return null;
            }

            ChatSession? existingSession = await _dbContext.ChatSessions.FindAsync(chatSession.Id);
            if (existingSession == null)
            {
                return null;
            }

            // Use SetValues to apply updates from the passed object to the entity
            _dbContext.Entry(existingSession).CurrentValues.SetValues(chatSession);
            await _dbContext.SaveChangesAsync();

            return existingSession;
        }

        // Delete ChatSession
        public async Task<bool> DeleteAsync(Guid id)
        {
            ChatSession? chatSession = await _dbContext.ChatSessions.FindAsync(id);
            if (chatSession == null)
            {
                return false;
            }

            _dbContext.ChatSessions.Remove(chatSession);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Get ChatSessions involving a specific User ID (either as Renter or Lender)
        public async Task<List<ChatSession>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.ChatSessions
                .Where(cs => cs.RenterId == userId || cs.LenderId == userId)
                .Include(cs => cs.Renter)
                .Include(cs => cs.Lender)
                .ToListAsync();
        }
    }
}