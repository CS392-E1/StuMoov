using StuMoov.Models.ChatModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Models.UserModel
{
    public class Lender : User
    {
        private Dictionary<Guid, StorageLocation>? StorageLocations {  get; set; }
        private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        //private Dictionary<Guid, BankInfo>? PayoutInfos { get; set; };  //Need to design PaymentInfo Class
        private Dictionary<Guid, ChatSession>? ChatSessions { get; set; }

        public StripeConnectAccount? StripeConnectInfo { get; private set; }

        public Lender(Guid id, string username, string email, string passwordHash) : base(id, username, email, passwordHash)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            Role = UserRole.LENDER;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
            StorageLocations = new Dictionary<Guid, StorageLocation>();
            RentalBookings = new Dictionary<Guid, Booking>();
            //PayoutInfos = new Dictionaty<Guid, BankInfo>();
            ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
