using StuMoov.Models.ChatModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel.Enums;
using Supabase.Postgrest.Attributes;
using System.Data;


namespace StuMoov.Models.UserModel
{
    public class Lender : User
    {
        //private Dictionary<Guid, StorageLocation>? StorageLocations {  get; set; }
        //private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        ////private Dictionary<Guid, BankInfo>? PayoutInfos { get; set; };  //Need to design PaymentInfo Class
        //private Dictionary<Guid, ChatSession>? ChatSessions { get; set; }
        [Reference(typeof(StripeConnectAccount))]
        public StripeConnectAccount? StripeConnectInfo { get; private set; }
        [Column("user_role")]
        public UserRole Role { get; private set; }

        public Lender() : base()
        {
        }

        public Lender(Guid id, string displayName, string email, string firebaseUid) : base(firebaseUid, email, displayName)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            Role = UserRole.LENDER;
            CreatedAt = DateTime.UtcNow;
            // RentalBookings = new Dictionary<Guid, Booking>();
            //PaymentInfos = new Dictionaty<Guid, PaymentInfo>();
            // ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
