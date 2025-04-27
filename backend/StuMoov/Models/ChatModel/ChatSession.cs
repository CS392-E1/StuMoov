using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
using StuMoov.Models.UserModel;
using StuMoov.Models.BookingModel;

namespace StuMoov.Models.ChatModel
{
    [Table("chat_sessions")]
    public class ChatSession : BaseModel
    {
        [Key]
        [PrimaryKey("id")]
        [Required]
        public Guid Id { get; private set; }
        [Column("renter_id")]
        public Guid RenterId { get; private set; }
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
        public Renter? Renter { get; private set; }
        [Column("lender_id")]
        [Required]
        public Guid LenderId { get; private set; }
        [Reference(typeof(Lender), ReferenceAttribute.JoinType.Inner, true, "lender_id")]
        public Lender? Lender { get; private set; }
        [Column("booking_id")]
        public Guid? BookingId { get; set; }
        [Reference(typeof(Booking), ReferenceAttribute.JoinType.Inner, true, "booking_id")]
        public Booking? Booking { get; private set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private ChatSession()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        public ChatSession(Renter renter, Lender lender)
        {
            Id = Guid.NewGuid();
            RenterId = renter.Id;
            LenderId = lender.Id;
            BookingId = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBooking(Booking booking)
        {
            BookingId = booking.Id;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
