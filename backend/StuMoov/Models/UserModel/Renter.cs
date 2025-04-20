using StuMoov.Models.BookingModel;
using StuMoov.Models.ChatModel;
using StuMoov.Models.UserModel.Enums;
using Supabase.Postgrest.Attributes;

namespace StuMoov.Models.UserModel
{
    public class Renter : User
    {
        // private Dictionary<Guid, Booking>? RentalBookings { get; set; }
        //private Dictionary<Guid, PaymentInfo>? PaymentInfos { get; set; };  //Need to design PaymentInfo Class
        // private Dictionary<Guid, ChatSession> ChatSessions { get; set; }
        [Reference(typeof(StripeCustomer))]
        public StripeCustomer? StripeCustomerInfo { get; private set; } // Stripe customer info for payment processing
        [Column("user_role")]
        public UserRole Role { get; private set; }

        public Renter() : base()
        {
        }

        public Renter(Guid id, string displayName, string email, string firebaseUid) : base(firebaseUid, email, displayName)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            Role = UserRole.RENTER;
            CreatedAt = DateTime.UtcNow;
            // RentalBookings = new Dictionary<Guid, Booking>();
            //PaymentInfos = new Dictionaty<Guid, PaymentInfo>();
            // ChatSessions = new Dictionary<Guid, ChatSession>();
        }
    }
}
