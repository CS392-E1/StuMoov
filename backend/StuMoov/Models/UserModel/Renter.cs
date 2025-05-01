/**
 * Renter.cs
 *
 * Represents a user with the role of renter who can rent storage locations,
 * pay via Stripe Customer, and participate in chat sessions.
 * Inherits from the base User class, mapped via Supabase Postgrest attributes
 * and compatible with EF Core.
 */

using StuMoov.Models.UserModel.Enums;
using Supabase.Postgrest.Attributes;

namespace StuMoov.Models.UserModel
{
    /// <summary>
    /// Represents a renter user in the system.
    /// </summary>
    public class Renter : User
    {
        /// <summary>
        /// The Stripe Customer Info ID linked to the renter for payment processing.
        /// </summary>
        [Column("StripeCustomerInfoId")]
        public Guid? StripeCustomerInfoId { get; private set; }

        /// <summary>
        /// The Stripe customer info associated with this renter.
        /// </summary>
        [Reference(typeof(StripeCustomer))]
        public StripeCustomer? StripeCustomerInfo { get; private set; } // Stripe customer info for payment processing

        /// <summary>
        /// Default constructor for Entity Framework and serialization.
        /// </summary>
        public Renter() : base()
        {
        }

        /// <summary>
        /// Constructs a Renter with specific identity and user information.
        /// </summary>
        /// <param name="id">The unique ID of the renter.</param>
        /// <param name="displayName">The display name of the renter.</param>
        /// <param name="email">The email address of the renter.</param>
        /// <param name="firebaseUid">The Firebase UID of the renter.</param>
        public Renter(Guid id, string displayName, string email, string firebaseUid) : base(firebaseUid, email, displayName)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            Role = UserRole.RENTER;
            CreatedAt = DateTime.UtcNow;
        }
    }
}