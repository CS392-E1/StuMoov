using StuMoov.Models.BookingModel;
using System.ComponentModel.DataAnnotations;

public class Booking
{
    public Guid Id { get; private set; }
    [Required]
    public Guid PaymentId { get; private set; }
    [Required]
    public Guid RenterId { get; private set; }
    [Required]
    public Guid StorageLocationId { get; private set; }
    [Required]
    public DateTime StartDate { get; set; }
    [Required]
    public DateTime EndDate { get; set; }
    public BookingStatus Status { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }

    public Booking(Guid renterId, Guid storageLocationId, DateTime startDate, DateTime endDate, decimal totalPrice)
    {
        Id = Guid.NewGuid();
        RenterId = renterId;
        StorageLocationId = storageLocationId;
        StartDate = startDate;
        EndDate = endDate;
        Status = BookingStatus.PENDING;
        TotalPrice = totalPrice;
    }
}
