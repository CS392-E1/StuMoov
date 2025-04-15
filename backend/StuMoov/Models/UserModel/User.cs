using StuMoov.Models.UserModel.Enums;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.UserModel
{
    public abstract class User
    {
        public Guid Id { get; protected set; }
        [Required]
        public string FirebaseUid { get; protected set; }
        [Required]
        public string Username { get; protected set; }
        [Required]
        public string Email { get; protected set; }
        [Required]
        public string PasswordHash { get; protected set; }
        [Required]
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public User(Guid id, string username, string email, string passwordHash)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
        }

    }
}
