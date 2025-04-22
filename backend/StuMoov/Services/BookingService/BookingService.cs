using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;

namespace StuMoov.Services.BookingService
{
    public class BookingService
    {
        [Required]
        private readonly BookingDao _bookingDao;

        // Constructor with dependency injection
        public BookingService(BookingDao bookingDao)
        {
            _bookingDao = bookingDao;
        }

        // Create a new booking - taking a Booking object directly
        public async Task<Response> CreateBookingAsync(Booking booking)
        {
            try
            {
                if (booking == null)
                    return new Response(StatusCodes.Status400BadRequest, "Booking cannot be null", null);

                // Validate the booking
                ValidateBooking(booking);

                // Check if the storage location is available for the requested dates
                if (!await _bookingDao.IsStorageLocationAvailableAsync(booking.StorageLocationId, booking.StartDate, booking.EndDate))
                    return new Response(StatusCodes.Status409Conflict, "The storage location is not available for the requested dates", null);

                var id = await _bookingDao.CreateAsync(booking);
                booking = await _bookingDao.GetByIdAsync(id);

                return new Response(StatusCodes.Status201Created, "Booking created successfully", booking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get a booking by ID
        public async Task<Response> GetBookingByIdAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = await _bookingDao.GetByIdAsync(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                return new Response(StatusCodes.Status200OK, "Booking retrieved successfully", booking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get all bookings
        public async Task<Response> GetAllBookingsAsync()
        {
            try
            {
                var bookings = await _bookingDao.GetAllAsync();
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by renter ID
        public async Task<Response> GetBookingsByRenterIdAsync(Guid renterId)
        {
            try
            {
                if (renterId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Renter ID cannot be empty", null);

                var bookings = await _bookingDao.GetByRenterIdAsync(renterId);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by storage location ID
        public async Task<Response> GetBookingsByStorageLocationIdAsync(Guid storageLocationId)
        {
            try
            {
                if (storageLocationId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Storage location ID cannot be empty", null);

                var bookings = await _bookingDao.GetByStorageLocationIdAsync(storageLocationId);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by status
        public async Task<Response> GetBookingsByStatusAsync(BookingStatus status)
        {
            try
            {
                var bookings = await _bookingDao.GetByStatusAsync(status);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Confirm a booking
        public async Task<Response> ConfirmBookingAsync(Guid id, Guid paymentId)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                if (paymentId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Payment ID cannot be empty", null);

                var booking = await _bookingDao.GetByIdAsync(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (booking.Status != BookingStatus.PENDING)
                    return new Response(StatusCodes.Status400BadRequest, "Only pending bookings can be confirmed", null);

                // Update booking status - we can't directly set the PaymentId in the new model
                // since it's a private setter. Instead, we'd need to create a new booking
                // or modify the model to allow this operation
                if (!await _bookingDao.UpdateStatusAsync(id, BookingStatus.CONFIRMED))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to update booking status", null);

                // Get the updated booking
                var updatedBooking = await _bookingDao.GetByIdAsync(id);
                return new Response(StatusCodes.Status200OK, "Booking confirmed successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Cancel a booking
        public async Task<Response> CancelBookingAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = await _bookingDao.GetByIdAsync(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (booking.Status == BookingStatus.CANCELLED)
                    return new Response(StatusCodes.Status400BadRequest, "Booking is already cancelled", null);

                if (!await _bookingDao.UpdateStatusAsync(id, BookingStatus.CANCELLED))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to cancel booking", null);

                // Get the updated booking
                var updatedBooking = await _bookingDao.GetByIdAsync(id);
                return new Response(StatusCodes.Status200OK, "Booking cancelled successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Update booking
        public async Task<Response> UpdateBookingAsync(Guid id, DateTime startDate, DateTime endDate, decimal totalPrice)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                if (totalPrice <= 0)
                    return new Response(StatusCodes.Status400BadRequest, "Total price must be greater than zero", null);

                var existingBooking = await _bookingDao.GetByIdAsync(id);
                if (existingBooking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (existingBooking.Status == BookingStatus.CANCELLED)
                    return new Response(StatusCodes.Status400BadRequest, "Cannot update a cancelled booking", null);

                // Check if new dates overlap with existing bookings (excluding the current booking)
                var storageLocationId = existingBooking.StorageLocationId;
                var overlappingBookings = (await _bookingDao.GetByStorageLocationIdAsync(storageLocationId))
                    .Where(b => b.Id != id && b.Status != BookingStatus.CANCELLED)
                    .Where(b =>
                        (b.StartDate >= startDate && b.StartDate < endDate) ||
                        (b.EndDate > startDate && b.EndDate <= endDate) ||
                        (b.StartDate <= startDate && b.EndDate >= endDate)
                    );

                if (overlappingBookings.Any())
                    return new Response(StatusCodes.Status409Conflict, "The requested dates conflict with existing bookings", null);

                // Update the booking
                if (!await _bookingDao.UpdateAsync(id, startDate, endDate, totalPrice))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to update booking", null);

                // Get the updated booking
                var updatedBooking = await _bookingDao.GetByIdAsync(id);
                return new Response(StatusCodes.Status200OK, "Booking updated successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get active bookings
        public async Task<Response> GetActiveBookingsAsync()
        {
            try
            {
                var bookings = await _bookingDao.GetActiveAsync();
                return new Response(StatusCodes.Status200OK, "Active bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings for a date range
        public async Task<Response> GetBookingsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                var bookings = await _bookingDao.GetForDateRangeAsync(startDate, endDate);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Check if a storage location is available for a date range
        public async Task<Response> IsStorageLocationAvailableAsync(Guid storageLocationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (storageLocationId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Storage location ID cannot be empty", null);

                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                bool isAvailable = await _bookingDao.IsStorageLocationAvailableAsync(storageLocationId, startDate, endDate);
                return new Response(
                    StatusCodes.Status200OK,
                    isAvailable ? "Storage location is available" : "Storage location is not available",
                    isAvailable
                );
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Calculate booking duration in days
        public async Task<Response> CalculateBookingDurationAsync(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = await _bookingDao.GetByIdAsync(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                int duration = (int)(booking.EndDate - booking.StartDate).TotalDays;
                return new Response(StatusCodes.Status200OK, "Booking duration calculated successfully", duration);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get all upcoming bookings (start date in the future)
        public async Task<Response> GetUpcomingBookingsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var activeBookings = await _bookingDao.GetActiveAsync();
                var bookings = activeBookings
                    .Where(b => b.StartDate > now)
                    .ToList();

                return new Response(StatusCodes.Status200OK, "Upcoming bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get current bookings (between start and end date)
        public async Task<Response> GetCurrentBookingsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var activeBookings = await _bookingDao.GetActiveAsync();
                var bookings = activeBookings
                    .Where(b => b.StartDate <= now && b.EndDate >= now)
                    .ToList();

                return new Response(StatusCodes.Status200OK, "Current bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get expired bookings (end date in the past)
        public async Task<Response> GetExpiredBookingsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var allBookings = await _bookingDao.GetAllAsync();
                var bookings = allBookings
                    .Where(b => b.EndDate < now)
                    .ToList();

                return new Response(StatusCodes.Status200OK, "Expired bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings that start within a specific number of days
        public async Task<Response> GetBookingsStartingWithinDaysAsync(int days)
        {
            try
            {
                if (days < 0)
                    return new Response(StatusCodes.Status400BadRequest, "Days must be a non-negative number", null);

                var now = DateTime.Now;
                var futureDate = now.AddDays(days);

                var activeBookings = await _bookingDao.GetActiveAsync();
                var bookings = activeBookings
                    .Where(b => b.StartDate >= now && b.StartDate <= futureDate)
                    .ToList();

                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Helper method to validate booking
        private Response ValidateBooking(Booking booking)
        {
            if (booking.RenterId == Guid.Empty)
                return new Response(StatusCodes.Status400BadRequest, "Renter ID cannot be empty", null);

            if (booking.StorageLocationId == Guid.Empty)
                return new Response(StatusCodes.Status400BadRequest, "Storage location ID cannot be empty", null);

            if (booking.StartDate >= booking.EndDate)
                return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

            if (booking.TotalPrice <= 0)
                return new Response(StatusCodes.Status400BadRequest, "Total price must be greater than zero", null);

            return new Response(StatusCodes.Status200OK, "Valid Booking", booking);
        }
    }
}
