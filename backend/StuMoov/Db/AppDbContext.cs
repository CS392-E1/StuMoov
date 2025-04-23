using Microsoft.EntityFrameworkCore;
using StuMoov.Models.UserModel;
using StuMoov.Models.ChatModel;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.UserModel.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StuMoov.Models.MessageModel;

namespace StuMoov.Db
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Renter> Renters { get; set; }
        public DbSet<Lender> Lenders { get; set; }
        public DbSet<StripeCustomer> StripeCustomers { get; set; }
        public DbSet<StripeConnectAccount> StripeConnectAccounts { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<StorageLocation> StorageLocations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<Supabase.Postgrest.ClientOptions>();

            var userRoleConverter = new ValueConverter<UserRole, string>(
                v => v.ToString(),        // Convert enum to string when saving
                v => (UserRole)Enum.Parse(typeof(UserRole), v)  // Convert string to enum when reading
            );

            // Apply the converter to the Role property
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(userRoleConverter);

            // Configure TPH inheritance
            modelBuilder.Entity<User>()
                .HasDiscriminator<UserRole>("Role")
                .HasValue<Renter>(UserRole.RENTER)
                .HasValue<Lender>(UserRole.LENDER);

            // Configure primary keys
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<StripeCustomer>().HasKey(sc => sc.Id);
            modelBuilder.Entity<StripeConnectAccount>().HasKey(sca => sca.Id);
            modelBuilder.Entity<ChatSession>().HasKey(cs => cs.Id);
            modelBuilder.Entity<ChatMessage>().HasKey(cm => cm.Id);
            modelBuilder.Entity<Booking>().HasKey(b => b.Id);
            modelBuilder.Entity<StorageLocation>().HasKey(sl => sl.Id);
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);

            // Configure relationships
            // StripeCustomer -> User
            modelBuilder.Entity<StripeCustomer>()
                .HasOne(sc => sc.User)
                .WithOne()
                .HasForeignKey<StripeCustomer>(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // StripeConnectAccount -> User
            modelBuilder.Entity<StripeConnectAccount>()
                .HasOne(sca => sca.User)
                .WithOne()
                .HasForeignKey<StripeConnectAccount>(sca => sca.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage -> ChatSession
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.ChatSession)
                .WithMany()
                .HasForeignKey(cm => cm.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ChatMessage -> User (Sender)
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany()
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatSession -> Renter
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Renter)
                .WithMany()
                .HasForeignKey(cs => cs.RenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatSession -> Lender
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Lender)
                .WithMany()
                .HasForeignKey(cs => cs.LenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Handle circular dependency between Booking and Payment
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> Renter
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Renter)
                .WithMany()
                .HasForeignKey(b => b.RenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> StorageLocation
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.StorageLocation)
                .WithMany()
                .HasForeignKey(b => b.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // StorageLocation -> User (Lender)
            modelBuilder.Entity<StorageLocation>()
                .HasOne(sl => sl.Lender)
                .WithMany()
                .HasForeignKey(sl => sl.LenderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment -> Renter
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Renter)
                .WithMany()
                .HasForeignKey(p => p.RenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Lender
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Lender)
                .WithMany()
                .HasForeignKey(p => p.LenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatSession -> Booking (optional relationship)
            modelBuilder.Entity<ChatSession>()
                .HasOne(cs => cs.Booking)
                .WithMany()
                .HasForeignKey(cs => cs.BookingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure enum conversions
            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .HasConversion<string>();

            modelBuilder.Entity<StripeConnectAccount>()
                .Property(sca => sca.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasConversion<string>();
        }
    }
}
