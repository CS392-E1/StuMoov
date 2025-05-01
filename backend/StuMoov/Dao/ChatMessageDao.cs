/**
 * ChatMessageDao.cs
 * 
 * Handles data access operations for ChatMessage entities including retrieval
 * and creation of chat messages. Uses Entity Framework Core for database interactions.
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
    public class ChatMessageDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for chat messages

        /// <summary>
        /// Initialize the ChatMessageDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for chat operations</param>
        public ChatMessageDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a chat message by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the chat message</param>
        /// <returns>The ChatMessage entity if found; otherwise null</returns>
        public async Task<ChatMessage?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChatMessages
                .Include(cm => cm.Sender)                    // Include the sender details
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }

        /// <summary>
        /// Retrieves all chat messages for a given chat session, ordered by creation time.
        /// </summary>
        /// <param name="sessionId">The GUID of the chat session</param>
        /// <returns>List of ChatMessage entities in chronological order</returns>
        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _dbContext.ChatMessages
                .Where(cm => cm.ChatSessionId == sessionId) // Filter by session ID
                .Include(cm => cm.Sender)                    // Include sender info
                .OrderBy(cm => cm.CreatedAt)                 // Order by timestamp ascending
                .ToListAsync();
        }

        /// <summary>
        /// Adds a new chat message to the database.
        /// </summary>
        /// <param name="message">The ChatMessage entity to add</param>
        /// <returns>The saved ChatMessage with generated fields populated, or null if input is null</returns>
        public async Task<ChatMessage?> AddAsync(ChatMessage message)
        {
            if (message == null)
            {
                return null;                                 // Guard clause for null input
            }

            await _dbContext.ChatMessages.AddAsync(message);  // Add new message
            await _dbContext.SaveChangesAsync();              // Persist changes

            return message;
        }

        /// <summary>
        /// Retrieves the most recent chat message for a given session.
        /// </summary>
        /// <param name="sessionId">The GUID of the chat session</param>
        /// <returns>The latest ChatMessage entity if any; otherwise null</returns>
        public async Task<ChatMessage?> GetLatestMessageBySessionIdAsync(Guid sessionId)
        {
            return await _dbContext.ChatMessages
                .Where(cm => cm.ChatSessionId == sessionId) // Filter by session
                .OrderByDescending(cm => cm.CreatedAt)      // Order by timestamp descending
                .FirstOrDefaultAsync();                     // Take the first (latest)
        }
    }
}