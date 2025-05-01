/**
 * Booking.cs
 *
 * Represents a booking record linking a renter to a storage location,
 * including payment, duration, pricing, and status. Mapped to the "bookings"
 * table via Supabase Postgrest attributes and compatible with EF Core.
 */

using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using StuMoov.Models.UserModel;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.StorageLocationModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.BookingModel
{
    [Table("bookings")]
    public class Booking : BaseModel
    {
        /// <summary>
        /// Unique identifier for the booking.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the associated payment.
        /// Nullable until payment is created.
        /// </summary>
        [Column("payment_id")]
        public Guid? PaymentId { get; set; }

        /// <summary>
        /// Navigation property to the payment entity.
        /// </summary>
        [Reference(typeof(Payment), ReferenceAttribute.JoinType.Inner, true, "payment_id")]
    public Payment? Payment { get; set; } // Reference to the payment associated with this booking

        /// <summary>
        /// Foreign key referencing the renter.
        /// </summary>
        [Column("renter_id")]
        [Required]
        public Guid RenterId { get; set; }

        /// <summary>
        /// Navigation property to the renter entity.
        /// </summary>
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
        public Renter? Renter { get; set; }

        /// <summary>
        /// Foreign key referencing the storage location.
        /// </summary>
        [Column("storage_id")]
        [Required]
        public Guid StorageLocationId { get; set; }

        /// <summary>
        /// Navigation property to the storage location entity.
        /// </summary>
        [Reference(typeof(StorageLocation), ReferenceAttribute.JoinType.Inner, true, "storage_id")]
        public StorageLocation? StorageLocation { get; set; }

        /// <summary>
        /// Start date of the booking period.
        /// </summary>
        [Column("start_date")]
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the booking period.
        /// </summary>
        [Column("end_date")]
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Current status of the booking, enumerated in BookingStatus
        /// </summary>
        [Column("status")]
        public BookingStatus Status { get; set; }

        /// <summary>
        /// Total price for the booking duration.
        /// </summary>
        [Column("total_price")]
        [Required]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Timestamp when the booking was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the booking was last updated.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Parameterless constructor for EF Core and Supabase.
        /// </summary>
        public Booking()
        {
            // Required for EF Core and Supabase deserialization
        }

        /// <summary>
        /// Creates a new Booking with initial values and a new GUID.
        /// Status defaults to PENDING.
        /// </summary>
        /// <param name="payment">Associated Payment entity</param>
        /// <param name="renter">Renter making the booking</param>
        /// <param name="storageLocation">StorageLocation being booked</param>
        /// <param name="startDate">Start date of the booking</param>
        /// <param name="endDate">End date of the booking</param>
        /// <param name="totalPrice">Total price calculated for the booking</param>
        public Booking(
            Payment payment,
            Renter renter,
            StorageLocation storageLocation,
            DateTime startDate,
            DateTime endDate,
            decimal totalPrice)
        {
            Id = Guid.NewGuid();
            PaymentId = payment.Id;
            RenterId = renter.Id;
            StorageLocationId = storageLocation.Id;
            StartDate = startDate;
            EndDate = endDate;
            Status = BookingStatus.PENDING;
            TotalPrice = totalPrice;
        }
    }
}
