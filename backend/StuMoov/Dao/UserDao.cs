using StuMoov.Models.UserModel;

namespace StuMoov.Dao
{
    public class UserDao
    {
        private Dictionary<Guid, User> Users;

        public UserDao() { 
            Users = new Dictionary<Guid, User>();

            // Mock Renter
            User renter1 = new Renter(
                Guid.NewGuid(),                      // FirebaseUid
                "Alice",                             // FirstName
                "Smith",                             // LastName
                "alice_renter",                      // Username
                "alice@example.com",                 // Email
                "hashed-password-alice"              // PasswordHash
            );

            // Mock Lender
            User lender = new Lender(
                Guid.NewGuid(),                      // FirebaseUid
                "John",                              // FirstName
                "Doe",                               // LastName
                "john_lender",                       // Username
                "john@example.com",                  // Email
                "hashed-password-john"               // PasswordHash
            );

            User renter2 = new Renter(
                Guid.NewGuid(),                      // FirebaseUid
                "Ludwig",                             // FirstName
                "Jake",                             // LastName
                "LDVG_renter",                      // Username
                "ldvg@example.com",                 // Email
                "hashed-password-ldvg"              // PasswordHash
            );
            Users[renter1.Id] = renter1;
            Users[renter2.Id] = renter2;
            Users[lender.Id] = lender;
        }

        public User? GetUserById(Guid id)
        {
            if (Users.TryGetValue(id, out var user))
            {
                return user;
            }
            return null;
        }

        // Get user by username
        public User? GetUserByUsername(string username)
        {
            return Users.Values.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // Get user by email
        public User? GetUserByEmail(string email)
        {
            return Users.Values.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // Add new user
        public User? AddUser(User user)
        {
            if (user == null || Users.ContainsKey(user.Id))
            {
                return null;
            }

            // Check if username or email already exists
            if (GetUserByUsername(user.Username) != null || GetUserByEmail(user.Email) != null)
            {
                return null;
            }

            Users[user.Id] = user;
            return user;
        }

        // Update existing user
        public User? UpdateUser(User user)
        {
            if (user == null || !Users.ContainsKey(user.Id))
            {
                return null;
            }

            Users[user.Id] = user;
            return user;
        }

        // Delete user
        public bool DeleteUser(Guid id)
        {
            return Users.Remove(id);
        }

        // Get all users
        public List<User> GetAllUsers()
        {
            return Users.Values.ToList();
        }

        // Get all renters
        public List<User> GetAllRenters()
        {
            return Users.Values.Where(u => u is Renter).ToList();
        }

        // Get all lenders
        public List<User> GetAllLenders()
        {
            return Users.Values.Where(u => u is Lender).ToList();
        }
    }
}
