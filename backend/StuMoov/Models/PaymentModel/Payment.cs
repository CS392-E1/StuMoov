using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Models.UserModel;

namespace StuMoov.Models.PaymentModel
{
    [Table("payments")]
    public class Payment : BaseModel
    {
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }
        [Column("booking_id")]
        [Required]
        public Guid BookingId { get; private set; }
        [Reference(typeof(Booking), ReferenceAttribute.JoinType.Inner, true, "booking_id")]
        public Booking? Booking { get; private set; } // Reference to the booking associated with this payment
        [Column("renter_id")]
        [Required]
        public Guid RenterId { get; private set; }
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
        public Renter? Renter { get; private set; } // Reference to the renter associated with this payment
        [Column("lender_id")]
        [Required]
        public Guid LenderId { get; private set; }
        [Reference(typeof(Lender), ReferenceAttribute.JoinType.Inner, true, "lender_id")]
        public Lender? Lender { get; private set; } // Reference to the lender associated with this payment
        [Required]
        [Column("stripe_payment_intent_id")]
        public string StripePaymentIntentId { get; private set; } = string.Empty;
        [Column("stripe_charge_id")]
        public string? StripeChargeId { get; private set; }
        [Column("stripe_transfer_id")]
        public string? StripeTransferId { get; private set; }
        [Column("amount_charged")]
        [Required]
        public decimal AmountCharged { get; private set; }
        [Column("currency")]
        [Required]
        public string Currency { get; private set; } = "usd";
        [Column("platform_fee")]
        [Required]
        public decimal PlatformFee { get; private set; }
        [Column("amount_transferred")]
        [Required]
        public decimal AmountTransferred { get; private set; }
        [Column("status")]
        [Required]
        public PaymentStatus Status { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }
        [Column("updated_at")]
        public DateTime UpdatedAt { get; private set; }

        // Constructor for EF Core - it needs one that only takes scalar values
        private Payment()
        {
            // This empty constructor is for EF Core
            // The private modifier restricts its usage to EF Core only
        }

        public Payment(Booking booking, Renter renter, Lender lender, string stripePaymentIntentId, decimal amountCharged, decimal platformFee, decimal amountTransferred)
        {
            Id = Guid.NewGuid();
            BookingId = booking.Id;
            RenterId = renter.Id;
            LenderId = lender.Id;
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
