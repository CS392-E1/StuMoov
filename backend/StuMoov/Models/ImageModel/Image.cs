using System.ComponentModel.DataAnnotations;
using StuMoov.Models.BookingModel;
using StuMoov.Models.StorageLocationModel;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace StuMoov.Models.ImageModel;

[Table("Images")]
public class Image : BaseModel
{
    [Key]
    [PrimaryKey("id")]
    public Guid Id { get; private set; }

    [Column("url")]
    [Required]
    public string Url { get; set; }

    [Column("storage_location_id")]
    public Guid? StorageLocationId { get; set; }

    [Column("booking_id")]
    public Guid? BookingId { get; set; }

    [Reference(typeof(StorageLocation), ReferenceAttribute.JoinType.Left, false, "storage_location_id")]
    public StorageLocation? StorageLocation { get; set; }

    [Reference(typeof(Booking), ReferenceAttribute.JoinType.Left, false, "booking_id")]
    public Booking? Booking { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }

    public Image() {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}