using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.ChatModel
{
    public class ChatSession
    {
        [Required]
        public Guid Id { get; private set; }
        [Required]
        public Guid RenterId { get; private set; }
        [Required]
        public Guid LenderId { get; private set; }
        public Guid? BookingId { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; set; }
        public ChatSession(Guid renterId, Guid lenderId, Guid bookingId) { 
            Id = Guid.NewGuid();
            RenterId = renterId;
            LenderId = lenderId;
            BookingId = bookingId;
            CreatedAt = DateTime.Now;
        }
    }
}
