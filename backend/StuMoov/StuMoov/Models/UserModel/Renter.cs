using StuMoov.Models.BookingModel;
using StuMoov.Models.ChatModel;

namespace StuMoov.Models.UserModel
{
    public class Renter : User
    {
        private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        //private Dictionary<Guid, PaymentInfo>? PaymentInfos { get; set; };  //Need to design PaymentInfo Class
        private Dictionary<Guid, ChatSession> ChatSessions { get; set; }

        public Renter(Guid id, string username, string email, string passwordHash) : base(id, username, email, passwordHash)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = UserRole.RENTER;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
            RentalBookings = new Dictionary<Guid, Booking>();
            //PaymentInfos = new Dictionaty<Guid, PaymentInfo>();
            ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
