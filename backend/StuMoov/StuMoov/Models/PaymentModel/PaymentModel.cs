using System.ComponentModel.DataAnnotations;

namespace StuMoov.Models.PaymentModel
{
    public class PaymentModel
    {
        public Guid Id { get; private set; }
        [Required]
        public Guid BookingId { get; private set; }
        [Required]
        public Guid RenterId { get; private set; }
        [Required]
        public Guid LenderId { get; private set; }
        [Required]
        public string StripePaymentIntentId { get; private set; } = string.Empty;
        public string? StripeChargeId { get; private set; }
        public string? StripeTransferId { get; private set; }
        [Required]
        public decimal AmountCharged { get; private set; }
        [Required]
        public string Currency { get; private set; } = "usd";
        [Required]
        public decimal PlatformFee { get; private set; }
        [Required]
        public decimal AmountTransferred { get; private set; }
        [Required]
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public PaymentModel(Guid bookingId, Guid renterId, Guid lenderId, string stripePaymentIntentId, decimal amountCharged, decimal platformFee, decimal amountTransferred)
        {
            Id = Guid.NewGuid();
            BookingId = bookingId;
            RenterId = renterId;
            LenderId = lenderId;
            StripePaymentIntentId = stripePaymentIntentId;
            AmountCharged = amountCharged;
            Currency = "usd";
            PlatformFee = platformFee;
            AmountTransferred = amountTransferred;
            Status = PaymentStatus.REQUIRES_ACTION;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

    }
}
