/**
 * Image.cs
 *
 * Represents an image associated with either a storage location or a booking.
 * Mapped to the "Images" table via Supabase Postgrest attributes and compatible
 * with EF Core conventions. Inherits from BaseModel for common properties.
 */

using System;
using System.ComponentModel.DataAnnotations;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;

namespace StuMoov.Models.ImageModel
{
    [Table("Images")]
    public class Image : BaseModel
    {
        /// <summary>
        /// Unique identifier for the image.
        /// Initialized in the parameterless constructor.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// URL or path to the image resource.
        /// </summary>
        [Column("url")]
        [Required]
        public string Url { get; set; }

        /// <summary>
        /// Optional foreign key referencing a storage location this image belongs to.
        /// </summary>
        [Column("storage_location_id")]
        public Guid? StorageLocationId { get; set; }

        /// <summary>
        /// Optional foreign key referencing a booking this image belongs to.
        /// </summary>
        [Column("booking_id")]
        public Guid? BookingId { get; set; }

        /// <summary>
        /// Navigation property for the storage location.
        /// Left join since the image may not belong to a storage location.
        /// </summary>
        [Reference(typeof(StorageLocation), ReferenceAttribute.JoinType.Left, false, "storage_location_id")]
        public StorageLocation? StorageLocation { get; set; }

        /// <summary>
        /// Navigation property for the booking.
        /// Left join since the image may not belong to a booking.
        /// </summary>
        [Reference(typeof(Booking), ReferenceAttribute.JoinType.Left, false, "booking_id")]
        public Booking? Booking { get; set; }

        /// <summary>
        /// Timestamp when the image record was created.
        /// Initialized in the constructor.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Constructs a new Image with a generated GUID and creation timestamp.
        /// </summary>
        public Image()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}