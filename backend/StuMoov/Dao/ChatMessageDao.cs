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
        private readonly AppDbContext _dbContext;

        public ChatMessageDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get ChatMessage by ID
        public async Task<ChatMessage?> GetByIdAsync(Guid id)
        {
            return await _dbContext.ChatMessages
                .Include(cm => cm.Sender)
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }

        // Get ChatMessages by ChatSession ID, ordered by timestamp
        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId)
        {
            return await _dbContext.ChatMessages
                .Where(cm => cm.ChatSessionId == sessionId)
                .Include(cm => cm.Sender)
                .OrderBy(cm => cm.CreatedAt)
                .ToListAsync();
        }

        // Add new ChatMessage
        public async Task<ChatMessage?> AddAsync(ChatMessage message)
        {
            if (message == null)
            {
                return null;
            }

            await _dbContext.ChatMessages.AddAsync(message);
            await _dbContext.SaveChangesAsync();

            return message;
        }

        // Get the latest message for a session
        public async Task<ChatMessage?> GetLatestMessageBySessionIdAsync(Guid sessionId)
        {
            return await _dbContext.ChatMessages
                .Where(cm => cm.ChatSessionId == sessionId)
                .OrderByDescending(cm => cm.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}