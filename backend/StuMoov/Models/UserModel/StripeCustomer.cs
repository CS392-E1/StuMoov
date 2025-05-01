using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.UserModel
{
    /// <summary>
    /// Represents a Stripe customer associated with a renter user.
    /// This entity manages Stripe customer data and payment methods for transactions.
    /// </summary>

    // This class helps us to manage Stripe customers and their payment methods
    // On create-account, an entry in the user and stripe_customer table is made for one user
    // stripe_customer table data is only filled if the user makes a purchase
    [Table("stripe_customers")]
    public class StripeCustomer : BaseModel
    {
        /// <summary>
        /// The unique identifier for the StripeCustomer entity.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// The ID of the user (renter) linked to this Stripe customer.
        /// </summary>
        [Column("user_id")]
        [Required]
        public Guid UserId { get; private set; }

        // This Reference attribute is used to create a navigation property to the User table
        // This allows us to access the User table data from the StripeCustomer table
        // by simply doing stripeCustomer.User
        // ref: https://supabase-community.github.io/supabase-csharp/api/Supabase.Postgrest.Attributes.ReferenceAttribute.html
        /// <summary>
        /// The renter user associated with this Stripe customer.
        /// Enables navigation to the User table via the Reference attribute.
        /// </summary>
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "user_id")]
        public Renter? User { get; private set; } // fk to User table

        /// <summary>
        /// The Stripe customer ID assigned by Stripe.
        /// Nullable as it is set only after a purchase is made.
        /// </summary>
        [Column("stripe_customer_id")]
        public string? StripeCustomerId { get; private set; }

        // Payment methods are stored in Stripe and can be retrieved using the Stripe Customer ID
        // ref: https://docs.stripe.com/api#retrieve_card
        // Users are able to add other payment methods, if so then we update the default with most
        // recently used payment method
        // ref: https://docs.stripe.com/api/cards/update
        /// <summary>
        /// The default payment method ID for the Stripe customer.
        /// Nullable as it is set only when a payment method is added.
        /// </summary>
        [Column("default_payment_method_id")]
        public string? DefaultPaymentMethodId { get; private set; }

        /// <summary>
        /// The date and time when this Stripe customer record was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// The date and time when this Stripe customer record was last updated.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Default constructor for Entity Framework Core.
        /// Private to restrict usage to EF Core only.
        /// </summary>
        private StripeCustomer()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        /// <summary>
        /// Constructs a StripeCustomer with the associated user.
        /// Initializes with null Stripe customer ID as it is set post-purchase.
        /// </summary>
        /// <param name="user">The renter user linked to this Stripe customer.</param>
        public StripeCustomer(User user)
        {
            Id = Guid.NewGuid();
            UserId = user.Id;
            StripeCustomerId = null;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates the Stripe customer ID and optional default payment method ID.
        /// Also updates the timestamp for the record.
        /// </summary>
        /// <param name="stripeCustomerId">The Stripe customer ID to set.</param>
        /// <param name="defaultPaymentMethodId">The default payment method ID, if applicable.</param>
        public void UpdateStripeInfo(string stripeCustomerId, string? defaultPaymentMethodId = null)
        {
            StripeCustomerId = stripeCustomerId;
            DefaultPaymentMethodId = defaultPaymentMethodId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
