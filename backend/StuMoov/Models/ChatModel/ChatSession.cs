/**
 * ChatSession.cs
 *
 * Represents a conversation session between a renter and lender, optionally tied to a booking or storage listing.
 * Mapped to the "chat_sessions" table via Supabase Postgrest attributes and compatible with EF Core.
 */

using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
using StuMoov.Models.UserModel;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;

namespace StuMoov.Models.ChatModel
{
    [Table("chat_sessions")]
    public class ChatSession : BaseModel
    {
        /// <summary>
        /// Unique identifier for the chat session.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        [Required]
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the renter participating in the session.
        /// </summary>
        [Column("renter_id")]
        public Guid RenterId { get; private set; }

        /// <summary>
        /// Navigation property to the Renter entity.
        /// </summary>
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
        public Renter? Renter { get; private set; }

        /// <summary>
        /// Foreign key referencing the lender participating in the session.
        /// </summary>
        [Column("lender_id")]
        [Required]
        public Guid LenderId { get; private set; }

        /// <summary>
        /// Navigation property to the Lender entity.
        /// </summary>
        [Reference(typeof(Lender), ReferenceAttribute.JoinType.Inner, true, "lender_id")]
        public Lender? Lender { get; private set; }

        /// <summary>
        /// Optional foreign key referencing the associated booking.
        /// </summary>
        [Column("booking_id")]
        public Guid? BookingId { get; set; }

        /// <summary>
        /// Navigation property to the Booking entity.
        /// </summary>
        [Reference(typeof(Booking), ReferenceAttribute.JoinType.Inner, true, "booking_id")]
        public Booking? Booking { get; private set; }

        /// <summary>
        /// Optional foreign key referencing a storage location listing.
        /// </summary>
        [Column("storage_location_id")]
        public Guid? StorageLocationId { get; set; }

        /// <summary>
        /// Navigation property to the StorageLocation entity.
        /// </summary>
        [Reference(typeof(StorageLocation), ReferenceAttribute.JoinType.Inner, true, "storage_location_id")]
        public StorageLocation? StorageLocation { get; private set; }

        /// <summary>
        /// Timestamp when the session was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Timestamp when the session was last updated.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Parameterless constructor for EF Core and Supabase deserialization.
        /// </summary>
        private ChatSession()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        /// <summary>
        /// Constructs a new ChatSession between a renter and lender for a given storage listing.
        /// </summary>
        /// <param name="renter">The renter participating in the chat</param>
        /// <param name="lender">The lender participating in the chat</param>
        /// <param name="storageLocation">The storage listing context for the chat</param>
        public ChatSession(Renter renter, Lender lender, StorageLocation storageLocation)
        {
            Id = Guid.NewGuid();
            RenterId = renter.Id;
            LenderId = lender.Id;
            BookingId = null;
            StorageLocationId = storageLocation.Id;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Associates this chat session with a confirmed booking and updates the timestamp.
        /// </summary>
        /// <param name="booking">The Booking entity to associate</param>
        public void SetBooking(Booking booking)
        {
            BookingId = booking.Id;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

