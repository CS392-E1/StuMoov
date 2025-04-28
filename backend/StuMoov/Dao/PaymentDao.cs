using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StuMoov.Db;
using StuMoov.Models.PaymentModel;

namespace StuMoov.Dao
{
    public class PaymentDao
    {
        [Required]
        private readonly AppDbContext _dbContext;

        public PaymentDao(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get Payment by ID
        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        // Get Payment by Booking ID
        public async Task<Payment?> GetByBookingIdAsync(Guid bookingId)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        // Get Payment by Stripe Invoice ID
        public async Task<Payment?> GetByStripeInvoiceIdAsync(string stripeInvoiceId)
        {
            return await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.StripeInvoiceId == stripeInvoiceId);
        }

        // Add new Payment
        public async Task<Payment?> AddAsync(Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            // Check if a payment already exists for this booking
            Payment? existingPayment = await GetByBookingIdAsync(payment.BookingId);
            if (existingPayment != null)
            {
                return null;
            }

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();

            return payment;
        }

        // Update Payment details 
        public async Task<Payment?> UpdatePaymentWithInvoiceDetailsAsync(Guid paymentId, string stripeInvoiceId, PaymentStatus status, decimal amountCharged, decimal platformFee, decimal amountTransferred)
        {
            Payment? payment = await GetByIdAsync(paymentId);
            if (payment == null)
            {
                return null;
            }

            payment.UpdateWithInvoiceDetails(stripeInvoiceId, status, amountCharged, platformFee, amountTransferred);
            await _dbContext.SaveChangesAsync();

            return payment;
        }

        // Update Payment status
        public async Task<Payment?> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus newStatus)
        {
            Payment? payment = await GetByIdAsync(paymentId);
            if (payment == null)
            {
                return null;
            }

            payment.UpdateStatus(newStatus);
            await _dbContext.SaveChangesAsync();

            return payment;
        }

        public async Task<Payment?> UpdateAsync(Payment payment)
        {
            if (payment == null)
            {
                return null;
            }

            Payment? existingPayment = await _dbContext.Payments.FindAsync(payment.Id);
            if (existingPayment == null)
            {
                return null;
            }

            _dbContext.Entry(existingPayment).CurrentValues.SetValues(payment);
            await _dbContext.SaveChangesAsync();

            return payment;
        }

        // Delete Payment
        public async Task<bool> DeleteAsync(Guid id)
        {
            Payment? payment = await _dbContext.Payments.FindAsync(id);
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