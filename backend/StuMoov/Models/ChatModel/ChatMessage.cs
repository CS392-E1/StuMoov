/**
 * ChatMessage.cs
 *
 * Represents an individual message within a chat session, including sender,
 * content, read status, and timestamps. Mapped to the "chat_messages" table
 * via Supabase Postgrest attributes and compatible with EF Core.
 */

using StuMoov.Models.UserModel;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.ChatModel
{
    [Table("chat_messages")]
    public class ChatMessage : BaseModel
    {
        /// <summary>
        /// Unique identifier for the chat message.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the chat session this message belongs to.
        /// </summary>
        [Column("chat_session_id")]
        [Required]
        public Guid ChatSessionId { get; private set; }

        /// <summary>
        /// Navigation property to the associated ChatSession.
        /// </summary>
        [Reference(typeof(ChatSession), ReferenceAttribute.JoinType.Inner, true, "chat_session_id")]
        public ChatSession ChatSession { get; private set; }

        /// <summary>
        /// Foreign key referencing the user who sent the message.
        /// </summary>
        [Column("sender_id")]
        [Required]
        public Guid SenderId { get; private set; }

        /// <summary>
        /// Navigation property to the sending User.
        /// </summary>
        [Reference(typeof(User), ReferenceAttribute.JoinType.Inner, true, "sender_id")]
        public User Sender { get; private set; }

        /// <summary>
        /// The textual content of the chat message.
        /// </summary>
        [Required]
        [Column("content")]
        public string Content { get; set; }

        /// <summary>
        /// Indicates whether the message has been read by the recipient.
        /// </summary>
        [Column("is_read")]
        public bool IsRead { get; set; }

        /// <summary>
        /// Timestamp when the message was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private ChatMessage()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        /// <summary>
        /// Constructs a new ChatMessage within a specified session, sender, and content.
        /// </summary>
        /// <param name="chatSession">The chat session this message belongs to</param>
        /// <param name="sender">The user sending the message</param>
        /// <param name="content">The message text</param>
        public ChatMessage(ChatSession chatSession, User sender, string content)
        {
            Id = Guid.NewGuid();
            ChatSessionId = chatSession.Id;
            SenderId = sender.Id;
            Content = content;
            IsRead = false;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
