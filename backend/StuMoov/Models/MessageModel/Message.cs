// Models/MessageModel/Message.cs
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.MessageModel
{
    [Table("messages")]
    public class Message : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column("sender_id")]     public Guid SenderId    { get; set; }
        [Column("recipient_id")]  public Guid RecipientId { get; set; }
        [Column("content")]       public string Content   { get; set; } = "";
        [Column("sent_at")]       public DateTime SentAt  { get; set; } = DateTime.UtcNow;
    }
}