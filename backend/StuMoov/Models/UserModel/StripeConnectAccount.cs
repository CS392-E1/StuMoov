using StuMoov.Models.UserModel.Enums;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.UserModel
{

    // ref: https://stackoverflow.com/questions/66254017/stripe-connect-api-how-do-i-retrieve-a-connected-accounts-status
    // Above is just a note for myself, we will have to implement a customer function to retrieve accounts status
    // This class helps us to manage Stripe connected accounts
    // We will only create entries in this table if the user is a lender (will have lender-specific auth stuff)
    public class StripeConnectAccount
    {
        public Guid Id { get; private set; }
        [Required]
        public Guid UserId { get; private set; }
        [Required]
        public string StripeConnectAccountId { get; private set; } = string.Empty;
        public StripeConnectAccountStatus Status { get; private set; }
        public bool PayoutsEnabled { get; private set; }
        public string? AccountLinkUrl { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }


        public StripeConnectAccount(Guid userId, string stripeConnectAccountId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            StripeConnectAccountId = stripeConnectAccountId;
            Status = StripeConnectAccountStatus.PENDING;
            PayoutsEnabled = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
