/**
 * Payment.cs
 *
 * Represents a payment record associated with a booking, including Stripe integration details,
 * amounts, fees, and status. Mapped to the "payments" table via Supabase Postgrest attributes
 * and compatible with EF Core. Inherits common properties from BaseModel.
 */

using System;
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
        /// <summary>
        /// Unique identifier of the payment.
        /// </summary>
        [Key]
        [PrimaryKey("id")]
        public Guid Id { get; private set; }

        /// <summary>
        /// Foreign key referencing the associated booking.
        /// </summary>
        [Column("booking_id")]
        [Required]
        public Guid BookingId { get; private set; }

        /// <summary>
        /// Navigation property to the Booking entity.
        /// </summary>
        [Reference(typeof(Booking), ReferenceAttribute.JoinType.Inner, true, "booking_id")]
        public Booking? Booking { get; private set; }

        /// <summary>
        /// Foreign key referencing the renter who paid.
        /// </summary>
        [Column("renter_id")]
        [Required]
        public Guid RenterId { get; private set; }

        /// <summary>
        /// Navigation property to the Renter entity.
        /// </summary>
        [Reference(typeof(Renter), ReferenceAttribute.JoinType.Inner, true, "renter_id")]
        public Renter? Renter { get; private set; }

        /// <summary>
        /// Foreign key referencing the lender to be paid.
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
        /// Stripe invoice identifier, if an invoice has been generated.
        /// </summary>
        [Column("stripe_invoice_id")]
        public string? StripeInvoiceId { get; private set; }

        /// <summary>
        /// Stripe payment intent identifier required for payment processing.
        /// </summary>
        [Column("stripe_payment_intent_id")]
        [Required]
        public string StripePaymentIntentId { get; private set; } = string.Empty;

        /// <summary>
        /// Stripe charge identifier, if a charge has occurred.
        /// </summary>
        [Column("stripe_charge_id")]
        public string? StripeChargeId { get; private set; }

        /// <summary>
        /// Stripe transfer identifier, if funds have been transferred.
        /// </summary>
        [Column("stripe_transfer_id")]
        public string? StripeTransferId { get; private set; }

        /// <summary>
        /// Total amount charged to the renter.
        /// </summary>
        [Column("amount_charged")]
        [Required]
        public decimal AmountCharged { get; private set; }

        /// <summary>
        /// Currency code of the transaction (e.g., "usd").
        /// </summary>
        [Column("currency")]
        [Required]
        public string Currency { get; private set; } = "usd";

        /// <summary>
        /// Platform fee deducted from the total amount.
        /// </summary>
        [Column("platform_fee")]
        [Required]
        public decimal PlatformFee { get; private set; }

        /// <summary>
        /// Amount transferred to the lender after fees.
        /// </summary>
        [Column("amount_transferred")]
        [Required]
        public decimal AmountTransferred { get; private set; }

        /// <summary>
        /// Current status of the payment (e.g., DRAFT, COMPLETED).
        /// </summary>
        [Column("status")]
        [Required]
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Timestamp when the payment record was created.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Timestamp when the payment record was last updated.
        /// </summary>
        [Column("updated_at")]
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core and Supabase.
        /// </summary>
        private Payment()
        {
            // Required for EF Core and Supabase deserialization
        }

        /// <summary>
        /// Constructs a new Payment with initial values and default status DRAFT.
        /// </summary>
        /// <param name="booking">Associated Booking entity</param>
        /// <param name="renter">Renter paying</param>
        /// <param name="lender">Lender receiving funds</param>
        /// <param name="stripeInvoiceId">Optional Stripe invoice ID</param>
        /// <param name="stripePaymentIntentId">Stripe payment intent ID</param>
        /// <param name="amountCharged">Total amount charged</param>
        /// <param name="platformFee">Fee amount for the platform</param>
        /// <param name="amountTransferred">Amount transferred to the lender</param>
        public Payment(
            Booking booking,
            Renter renter,
            Lender lender,
            string? stripeInvoiceId,
            string stripePaymentIntentId,
            decimal amountCharged,
            decimal platformFee,
            decimal amountTransferred)
        {
            Id = Guid.NewGuid();
            BookingId = booking.Id;
            RenterId = renter.Id;
            LenderId = lender.Id;
            StripeInvoiceId = stripeInvoiceId;
            StripePaymentIntentId = stripePaymentIntentId;
            AmountCharged = amountCharged;
            Currency = "usd";
            PlatformFee = platformFee;
            AmountTransferred = amountTransferred;
            Status = PaymentStatus.DRAFT;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates payment details after creating a Stripe Invoice.
        /// </summary>
        /// <param name="stripeInvoiceId">New Stripe invoice ID</param>
        /// <param name="status">New payment status</param>
        /// <param name="amountCharged">Updated total amount charged</param>
        /// <param name="platformFee">Updated platform fee</param>
        /// <param name="amountTransferred">Updated amount transferred to lender</param>
        public void UpdateWithInvoiceDetails(
            string stripeInvoiceId,
            PaymentStatus status,
            decimal amountCharged,
            decimal platformFee,
            decimal amountTransferred)
        {
            StripeInvoiceId = stripeInvoiceId;
            Status = status;
            AmountCharged = amountCharged;
            PlatformFee = platformFee;
            AmountTransferred = amountTransferred;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Updates only the payment status, typically via webhook events.
        /// </summary>
        /// <param name="newStatus">The updated payment status</param>
        public void UpdateStatus(PaymentStatus newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}