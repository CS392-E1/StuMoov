using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.UserModel
{

    // This class helps us to manage Stripe customers and their payment methods
    // On create-account, an entry in the user and stripe_customer table is made for one user
    // stripe_customer table data is only filled if the user makes a purchase
    [Table("stripe_customers")]
    public class StripeCustomer : BaseModel
    {
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }
        [Column("user_id")]
        [Required]
        public Guid UserId { get; private set; }
        // This Reference attribute is used to create a navigation property to the User table
        // This allows us to access the User table data from the StripeCustomer table
        // by simply doing stripeCustomer.User
        // ref: https://supabase-community.github.io/supabase-csharp/api/Supabase.Postgrest.Attributes.ReferenceAttribute.html
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "user_id")]
        public Renter? User { get; private set; } // fk to User table
        [Column("stripe_customer_id")]
        public string? StripeCustomerId { get; private set; }

        // Payment methods are stored in Stripe and can be retrieved using the Stripe Customer ID
        // ref: https://docs.stripe.com/api#retrieve_card
        // Users are able to add other payment methods, if so then we update the default with most
        // recently used payment method
        // ref: https://docs.stripe.com/api/cards/update
        [Column("default_payment_method_id")]
        public string? DefaultPaymentMethodId { get; private set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; private set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private StripeCustomer()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        public StripeCustomer(User user)
        {
            Id = Guid.NewGuid();
            UserId = user.Id;
            StripeCustomerId = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Method to update Stripe information when it becomes available
        public void UpdateStripeInfo(string stripeCustomerId, string? defaultPaymentMethodId = null)
        {
            StripeCustomerId = stripeCustomerId;
            DefaultPaymentMethodId = defaultPaymentMethodId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
