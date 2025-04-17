using StuMoov.Models.ChatModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Models.UserModel
{
    public class Lender : User
    {
        private Dictionary<Guid, StorageLocation>? StorageLocations {  get; set; }
        private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        private Dictionary<Guid, ChatSession>? ChatSessions { get; set; }

        public StripeConnectAccount? StripeConnectInfo { get; private set; }

        public Lender(Guid firebaseUid, string firstName, string lastName, string username, string email, string passwordHash) : base(firebaseUid, firstName, lastName, username, email, passwordHash)
        {
            Id = Guid.NewGuid();
            FirebaseUid = firebaseUid;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = UserRole.LENDER;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
            StorageLocations = new Dictionary<Guid, StorageLocation>();
            RentalBookings = new Dictionary<Guid, Booking>();
            ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
