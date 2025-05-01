/**
 * Lender.cs
 *
 * Represents a user with the role of lender who can list storage locations,
 * receive payments via Stripe Connect, and participate in chat sessions.
 * Inherits from the base User class, mapped via Supabase Postgrest attributes
 * and compatible with EF Core.
 */

using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using StuMoov.Models.ChatModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Models.UserModel
{
    /// <summary>
    /// A lender user who owns storage listings and can receive payouts via Stripe Connect.
    /// </summary>
    public class Lender : User
    {
        /// <summary>
        /// Foreign key referencing the Stripe Connect account details for this lender.
        /// </summary>
        [Column("StripeConnectInfoId")]
        public Guid? StripeConnectInfoId { get; private set; }

        /// <summary>
        /// Navigation property to the StripeConnectAccount entity.
        /// </summary>
        [Reference(typeof(StripeConnectAccount))]
        public StripeConnectAccount? StripeConnectInfo { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core and Supabase.
        /// </summary>
        public Lender() : base()
        {
        }

        /// <summary>
        /// Constructs a new Lender with explicit identifier and user fields.
        /// </summary>
        /// <param name="id">The unique identifier for the user (GUID).</param>
        /// <param name="displayName">The display name of the user.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="firebaseUid">The Firebase user identifier.</param>
        public Lender(Guid id, string displayName, string email, string firebaseUid)
            : base(firebaseUid, email, displayName)
        {
            Id = id;
            DisplayName = displayName;
            Email = email;
            Role = UserRole.LENDER;
            CreatedAt = DateTime.UtcNow;
        }
    }
}