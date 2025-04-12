using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.ChatModel
{
    public class ChatMessage
    {
        public Guid Id { get; private set; }
        [Required]
        public Guid ChatSessionId { get; private set; }
        [Required]
        public Guid SenderId { get; private set; }
        [Required]
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public ChatMessage(Guid chatSessionId, Guid senderId, string content) {
            Id = Guid.NewGuid();
            ChatSessionId = chatSessionId;
            SenderId = senderId;
            Content = content;
            IsRead = false;
            CreatedAt = DateTime.Now;
        }
    }
}
