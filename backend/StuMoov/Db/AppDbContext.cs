/**
 * AppDbContext.cs
 *
 * Defines the Entity Framework Core database context for the StuMoov application,
 * including DbSet properties for each entity and model configuration in OnModelCreating.
 */

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StuMoov.Models.UserModel;
using StuMoov.Models.ChatModel;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.UserModel.Enums;
using StuMoov.Models.ImageModel;

namespace StuMoov.Db
{
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Creates a new instance of AppDbContext with the provided options.
        /// </summary>
        /// <param name="options">Configuration options for the context</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        // DbSets for application entities
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
        public DbSet<Image> Images { get; set; }

        /// <summary>
        /// Configures the EF Core model, including table mappings, relationships,
        /// inheritance, and enum conversions.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ignore Supabase client options type
            modelBuilder.Ignore<Supabase.Postgrest.ClientOptions>();

            // Value converter for UserRole enum
            var userRoleConverter = new ValueConverter<UserRole, string>(
                v => v.ToString(),
                v => (UserRole)Enum.Parse(typeof(UserRole), v)
            );

            // Apply the converter to the Role property
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion(userRoleConverter);

            // Configure Table-per-Hierarchy (TPH) inheritance for User
            modelBuilder.Entity<User>()
                .HasDiscriminator<UserRole>("Role")
                .HasValue<Renter>(UserRole.RENTER)
                .HasValue<Lender>(UserRole.LENDER);

            // Configure primary keys for all entities
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<StripeCustomer>().HasKey(sc => sc.Id);
            modelBuilder.Entity<StripeConnectAccount>().HasKey(sca => sca.Id);
            modelBuilder.Entity<ChatSession>().HasKey(cs => cs.Id);
            modelBuilder.Entity<ChatMessage>().HasKey(cm => cm.Id);
            modelBuilder.Entity<Booking>().HasKey(b => b.Id);
            modelBuilder.Entity<StorageLocation>().HasKey(sl => sl.Id);
            modelBuilder.Entity<Payment>().HasKey(p => p.Id);
            modelBuilder.Entity<Image>().HasKey(i => i.Id);

            // Configure relationships and cascade behaviors
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

            // Configure enum-to-string conversions for BookingStatus, StripeConnectAccountStatus, and PaymentStatus
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