using StuMoov.Models.UserModel.Enums;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.UserModel
{
    public abstract class User
    {
        public Guid Id { get; protected set; }
        [Required]
        public Guid FirebaseUid { get; protected set; }
        [Required]
        public string FirstName { get; set; }
        [Required] 
        public string LastName { get; set;}
        [Required]
        public string Username { get; protected set; }
        [Required]
        public string Email { get; protected set; }
        [Required]
        public string PasswordHash { get; protected set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        public User(Guid firebaseUid, string firstName, string lastName, string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            FirebaseUid = firebaseUid;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            IsActive = false;  //Need to go through OAuth
            CreatedAt = DateTime.UtcNow;
        }

    }
}
