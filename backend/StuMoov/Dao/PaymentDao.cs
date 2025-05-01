/**
 * PaymentDao.cs
 * 
 * Handles data access operations for Payment entities including retrieval,
 * creation, update, and deletion. Uses Entity Framework Core for database interactions.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.PaymentModel;

namespace StuMoov.Dao
{
    public class PaymentDao
    {
        [Required]
        private readonly AppDbContext _dbContext;  // EF Core database context for payments

        /// <summary>
        /// Initialize the PaymentDao with the required AppDbContext dependency.
        /// </summary>
        /// <param name="dbContext">EF Core database context for payment operations</param>
        public PaymentDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Retrieves a payment by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the payment</param>
        /// <returns>The Payment entity if found; otherwise null</returns>
        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Retrieves a payment associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">The GUID of the booking</param>
        /// <returns>The Payment entity if found; otherwise null</returns>
        public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        /// <summary>
        /// Retrieves a payment by its Stripe invoice identifier.
        /// </summary>
        /// <param name="stripeInvoiceId">The Stripe invoice ID</param>
        /// <returns>The Payment entity if found; otherwise null</returns>
        public async Task<Payment?> GetByStripeInvoiceIdAsync(string stripeInvoiceId)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.StripeInvoiceId == stripeInvoiceId);
        }

        /// <summary>
        /// Adds a new payment record if none exists for the given booking.
        /// </summary>
        /// <param name="payment">The Payment entity to add</param>
        /// <returns>The saved Payment with generated fields populated, or null if invalid or duplicate</returns>
        public async Task<Payment?> AddAsync(Payment payment)
        {
            if (payment == null)
            {
                return null;  // Guard clause for null input
            }

            // Prevent duplicate payment for the same booking
            var existing = await GetByBookingIdAsync(payment.BookingId);
            if (existing != null)
            {
                return null;
            }

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();
            return payment;
        }

        /// <summary>
        /// Updates payment details with Stripe invoice information.
        /// </summary>
        /// <param name="paymentId">The GUID of the payment to update</param>
        /// <param name="stripeInvoiceId">The Stripe invoice ID</param>
        /// <param name="status">The new payment status</param>
        /// <param name="amountCharged">Total amount charged</param>
        /// <param name="platformFee">Platform fee amount</param>
        /// <param name="amountTransferred">Amount transferred to the lender</param>
        /// <returns>The updated Payment entity if found; otherwise null</returns>
        public async Task<Payment?> UpdatePaymentWithInvoiceDetailsAsync(
            Guid paymentId,
            string stripeInvoiceId,
            PaymentStatus status,
            decimal amountCharged,
            decimal platformFee,
            decimal amountTransferred)
        {
            var payment = await GetByIdAsync(paymentId);
            if (payment == null)
            {
                return null;
            }

            payment.UpdateWithInvoiceDetails(stripeInvoiceId, status, amountCharged, platformFee, amountTransferred);
            await _dbContext.SaveChangesAsync();
            return payment;
        }

        /// <summary>
        /// Updates only the status of an existing payment.
        /// </summary>
        /// <param name="paymentId">The GUID of the payment to update</param>
        /// <param name="newStatus">The new payment status</param>
        /// <returns>The updated Payment entity if found; otherwise null</returns>
        public async Task<Payment?> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
        {
            var payment = await GetByIdAsync(paymentId);
            if (payment == null)
            {
                return null;
            }

            payment.UpdateStatus(newStatus);
            await _dbContext.SaveChangesAsync();
            return payment;
        }

        /// <summary>
        /// Updates all editable fields of an existing payment.
        /// </summary>
        /// <param name="payment">Payment model containing updated values</param>
        /// <returns>The updated Payment entity if found; otherwise null</returns>
        public async Task<Payment?> UpdateAsync(Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            var existing = await _dbContext.Payments.FindAsync(payment.Id);
            if (existing == null)
            {
                return null;
            }

            _dbContext.Entry(existing).CurrentValues.SetValues(payment);
            await _dbContext.SaveChangesAsync();
            return payment;
        }

        /// <summary>
        /// Deletes a payment by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the payment to delete</param>
        /// <returns>True if deletion succeeded; otherwise false</returns>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var payment = await _dbContext.Payments.FindAsync(id);
            if (payment == null)
            {
                return false;
            }

            _dbContext.Payments.Remove(payment);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}