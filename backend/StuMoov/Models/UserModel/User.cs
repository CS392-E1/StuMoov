using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.UserModel
{
    [Table("users")]
    public abstract class User : BaseModel
    {
        [Key]
        [Column("id")]

        [PrimaryKey("id")]
        public Guid Id { get; protected set; }
        [Column("firebase_uid")]
        [Required]
        public string FirebaseUid { get; set; }
        [Column("display_name")]
        public string? DisplayName { get; set; }
        [Column("email")]
        [Required]
        public string Email { get; set; }
        // Reasoning behind commenting this out is that our Renter and Lender
        // models already have a Role property. Adding the UserRole property here
        // would duplicate the property in the DB
        //[Column("user_role")] 
        //[Required]
        //public UserRole Role { get; protected set; }
        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        protected User()
        {
        }

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
