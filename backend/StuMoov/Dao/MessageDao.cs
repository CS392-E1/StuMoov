using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.MessageModel;

public class MessageDao
{
    private readonly AppDbContext _context;

    public MessageDao(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Message>> GetMessagesAsync(Guid user1, Guid user2)
    {
        return await _context.Messages
            .Where(m =>
                (m.SenderId == user1 && m.RecipientId == user2) ||
                (m.SenderId == user2 && m.RecipientId == user1))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<Guid> CreateAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message.Id;
    }
}