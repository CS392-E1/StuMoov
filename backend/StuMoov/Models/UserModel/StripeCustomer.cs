using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.UserModel
{

    // This class helps us to manage Stripe customers and their payment methods
    // On create-account, an entry in the user and stripe_customer table is made for one user
    // stripe_customer table data is only filled if the user makes a purchase
    public class StripeCustomer
    {
        public Guid Id { get; private set; }
        [Required]
        public Guid UserId { get; private set; } // fk to User table
        [Required]
        public string StripeCustomerId { get; private set; } = string.Empty;

        // Payment methods are stored in Stripe and can be retrieved using the Stripe Customer ID
        // ref: https://docs.stripe.com/api#retrieve_card
        // Users are able to add other payment methods, if so then we update the default with most
        // recently used payment method
        // ref: https://docs.stripe.com/api/cards/update
        public string? DefaultPaymentMethodId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public StripeCustomer(Guid userId, string stripeCustomerId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            StripeCustomerId = stripeCustomerId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
