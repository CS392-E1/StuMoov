using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.UserModel.UserRecords
{
    [Table("users")]
    public class UserRecords : BaseModel
    {
        public UserRecords() { }           // parameterless ctor

        [PrimaryKey("id"), Column("id")]
        public Guid Id { get; set; }

        [Column("firebase_uid")]
        public string FirebaseUid { get; set; } = "";

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("user_role")]
        public string Role { get; set; } = "";

        [Column("is_email_verified")]
        public bool IsEmailVerified { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
