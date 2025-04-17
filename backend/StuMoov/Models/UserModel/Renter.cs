using StuMoov.Models.BookingModel;
using StuMoov.Models.ChatModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Models.UserModel
{
    public class Renter : User
    {
        private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        private Dictionary<Guid, ChatSession> ChatSessions { get; set; }
        public StripeCustomer? StripeCustomerInfo { get; private set; } // Stripe customer info for payment processing


        public Renter(Guid firebaseUid, string firstName, string lastName, string username, string email, string passwordHash) : base(firebaseUid, firstName, lastName, username, email, passwordHash)
        {
            Id = Guid.NewGuid();
            FirebaseUid = firebaseUid;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = UserRole.RENTER;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
            RentalBookings = new Dictionary<Guid, Booking>();
            ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
