using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using StuMoov.Models.UserModel;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.StorageLocationModel;
using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.BookingModel;
[Table("bookings")]
public class Booking : BaseModel
{
    [Key]
    [PrimaryKey("id")]
    public Guid Id { get; private set; }
    [Column("payment_id")]
    [Required]
    public Guid PaymentId { get; private set; }
    [Reference(typeof(Payment), ReferenceAttribute.JoinType.Inner, true, "payment_id")]
    public Payment? Payment { get; private set; } // Reference to the payment associated with this booking
    [Column("renter_id")]
    [Required]
    public Guid RenterId { get; private set; }
    [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
    public Renter? Renter { get; private set; } // Reference to the renter associated with this booking
    [Column("storage_id")]
    [Required]
    public Guid StorageLocationId { get; private set; }
    [Reference(typeof(StorageLocation), ReferenceAttribute.JoinType.Inner, true, "storage_id")]
    public StorageLocation? StorageLocation { get; private set; } // Reference to the storage location associated with this booking
    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; }
    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; }
    [Column("status")]
    public BookingStatus Status { get; set; }
    [Column("total_price")]
    [Required]
    public decimal TotalPrice { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; private set; }
    [Column("updated_at")]
    public DateTime UpdatedAt { get; private set; }

    // Constructor for EF Core - it needs one that only takes scalar values
    private Booking()
    {
        // This empty constructor is for EF Core
        // The private modifier restricts its usage to EF Core only
    }

    public Booking(Payment payment, Renter renter, StorageLocation storageLocation, DateTime startDate, DateTime endDate, decimal totalPrice)
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
