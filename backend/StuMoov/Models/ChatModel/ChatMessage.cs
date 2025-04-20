using StuMoov.Models.UserModel;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.ChatModel
{
    [Table("chat_messages")]
    public class ChatMessage : BaseModel
    {
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }
        [Column("chat_session_id")]
        [Required]
        public Guid ChatSessionId { get; private set; }
        [Reference(typeof(ChatSession), ReferenceAttribute.JoinType.Inner, true, "chat_session_id")]
        public ChatSession ChatSession { get; private set; }
        [Column("sender_id")]
        [Required]
        public Guid SenderId { get; private set; }
        [Reference(typeof(User), ReferenceAttribute.JoinType.Inner, true, "sender_id")]
        public User Sender { get; private set; }
        [Required]
        [Column("content")]
        public string Content { get; set; }
        [Column("is_read")]
        public bool IsRead { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private ChatMessage()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        public ChatMessage(ChatSession chatSession, User sender, string content) {
            Id = Guid.NewGuid();
            ChatSessionId = chatSession.Id;
            SenderId = sender.Id;
            Content = content;
            IsRead = false;
            CreatedAt = DateTime.Now;
        }
    }
}
