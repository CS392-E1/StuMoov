using StuMoov.Models.UserModel.Enums;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.UserModel
{

    // ref: https://stackoverflow.com/questions/66254017/stripe-connect-api-how-do-i-retrieve-a-connected-accounts-status
    // Above is just a note for myself, we will have to implement a customer function to retrieve accounts status
    // This class helps us to manage Stripe connected accounts
    // We will only create entries in this table if the user is a lender (will have lender-specific auth stuff)
    [Table("stripe_connect_accounts")]
    public class StripeConnectAccount : BaseModel
    {
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }
        [Required]
        [Column("user_id")]
        public Guid UserId { get; private set; }
        [Reference(typeof(Lender), ReferenceAttribute.JoinType.Inner, true, "user_id")]
        public Lender? User { get; private set; } // fk to User table
        [Column("stripe_connect_account_id")]
        [Required]
        public string StripeConnectAccountId { get; private set; }
        [Column("status")]
        public StripeConnectAccountStatus Status { get; private set; }
        [Column("payouts_enabled")]
        public bool PayoutsEnabled { get; private set; }
        [Column("account_link_url")]
        public string? AccountLinkUrl { get; private set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private StripeConnectAccount()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        public StripeConnectAccount(User user, string stripeConnectAccountId)
        {
            Id = Guid.NewGuid();
            UserId = user.Id;
            StripeConnectAccountId = stripeConnectAccountId;
            Status = StripeConnectAccountStatus.PENDING;
            PayoutsEnabled = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Method to update status
        public void UpdateStatus(StripeConnectAccountStatus status, bool payoutsEnabled)
        {
            Status = status;
            PayoutsEnabled = payoutsEnabled;
            UpdatedAt = DateTime.UtcNow;
        }

        // Method to update account link URL
        public void UpdateAccountLinkUrl(string accountLinkUrl)
        {
            AccountLinkUrl = accountLinkUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
