/**
 * User.cs
 *
 * Represents the base user entity for the StuMoov application.
 * This abstract model defines common properties for all user types (e.g., Renter, Lender).
 * Mapped using Supabase Postgrest attributes and compatible with EF Core.
 */

using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using StuMoov.Models.UserModel.Enums;

namespace StuMoov.Models.UserModel
{
    /// <summary>
    /// Abstract base class representing a user in the StuMoov application.
    /// Contains common user properties and serves as a base for specific user types.
    /// </summary>
    [Table("users")]
    public abstract class User : BaseModel
    {
        /// <summary>
        /// The unique identifier for the User entity.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        [Column("id")]
        public Guid Id { get; protected set; }

        /// <summary>
        /// The Firebase UID associated with the user for authentication.
        /// </summary>
        [Required]
        [Column("firebase_uid")]
        public string FirebaseUid { get; set; }

        /// <summary>
        /// The display name of the user, optional.
        /// Defaults to the user's email if not provided.
        /// </summary>
        [Column("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The email address of the user.
        /// </summary>
        [Required]
        [Column("email")]
        public string Email { get; set; }

        /// <summary>
        /// The role of the user (e.g., Renter, Lender).
        /// Defined in derived classes to avoid duplication in the database.
        /// </summary>
        [Column("Role")]
        public UserRole Role { get; protected set; }

        /// <summary>
        /// Indicates whether the user's email has been verified.
        /// </summary>
        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// The date and time when the user record was created.
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// The date and time when the user record was last updated.
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        protected User()
        {
        }

        /// <summary>
        /// Constructs a User with the specified properties.
        /// </summary>
        /// <param name="firebaseUid">The Firebase UID for authentication.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="displayName">The optional display name, defaults to email if null.</param>
        public User(string firebaseUid, string email, string? displayName = null)
        {
            Id = Guid.NewGuid();
            FirebaseUid = firebaseUid;
            Email = email;
            DisplayName = displayName ?? email; // Will display email if no displayName is set
            IsEmailVerified = false;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}