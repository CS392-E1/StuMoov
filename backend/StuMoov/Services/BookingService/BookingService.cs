using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Services.StripeService;
using Microsoft.Extensions.Logging;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel;
using StuMoov.Controllers;

namespace StuMoov.Services.BookingService
{
    public class BookingService
    {
        [Required]
        private readonly BookingDao _bookingDao;
        private readonly StuMoov.Services.StripeService.StripeService _stripeService;
        private readonly ILogger<BookingService> _logger;

        // Constructor with dependency injection
        public BookingService(BookingDao bookingDao, StuMoov.Services.StripeService.StripeService stripeService, ILogger<BookingService> logger)
        {
            _bookingDao = bookingDao;
            _stripeService = stripeService;
            _logger = logger;
        }

        // Updated CreateBookingAsync to accept DTO and fetched entities
        public async Task<Response> CreateBookingAsync(CreateBookingRequest request, Renter renter, StorageLocation storageLocation)
        {
            try
            {
                // Validate inputs (DTO/entities already validated in Controller, but can add more service-level checks)
                if (request == null)
                    return new Response(StatusCodes.Status400BadRequest, "Booking request cannot be null", null);
                if (renter == null)
                    return new Response(StatusCodes.Status400BadRequest, "Renter cannot be null", null);
                if (storageLocation == null)
                    return new Response(StatusCodes.Status400BadRequest, "Storage location cannot be null", null);

                if (request.StartDate >= request.EndDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                if (request.TotalPrice <= 0)
                    return new Response(StatusCodes.Status400BadRequest, "Total price must be greater than zero", null);

                // Check if the storage location is available for the requested dates
                if (!await _bookingDao.IsStorageLocationAvailableAsync(storageLocation.Id, request.StartDate, request.EndDate))
                    return new Response(StatusCodes.Status409Conflict, "The storage location is not available for the requested dates", null);

                // Construct the Booking object using the available constructor
                // Assuming the constructor sets the default status to PENDING or DAO handles it.
                // The Payment is null initially.
                Booking newBooking = new Booking(
                    payment: null,
                    renter: renter,
                    storageLocation: storageLocation,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    totalPrice: request.TotalPrice // Price in cents
                );

                var now = DateTime.UtcNow;
                newBooking.CreatedAt = now;
                newBooking.UpdatedAt = now;

                // Use the DAO to create the booking
                // Ensure the DAO's CreateAsync correctly sets the default PENDING status
                var id = await _bookingDao.CreateAsync(newBooking);
                var createdBooking = await _bookingDao.GetByIdAsync(id);

                return new Response(StatusCodes.Status201Created, "Booking created successfully", createdBooking);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating booking: {ex}");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the booking.", null);
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

        // Confirm a booking and trigger invoice creation
        public async Task<Response> ConfirmBookingAsync(Guid id)
        {
            _logger.LogInformation($"Attempting to confirm Booking ID: {id}");
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("ConfirmBookingAsync called with empty Booking ID.");
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);
                }

                var booking = await _bookingDao.GetByIdAsync(id);
                if (booking == null)
                {
                    _logger.LogWarning($"Booking with ID {id} not found for confirmation.");
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);
                }
                if (booking.PaymentId == Guid.Empty)
                {
                    _logger.LogError($"Booking {id} is missing associated Payment record. Cannot confirm.");
                    return new Response(StatusCodes.Status500InternalServerError, "Booking is missing payment information.", null);
                }

                if (booking.Status != BookingStatus.PENDING)
                {
                    _logger.LogWarning($"Attempted to confirm booking {id} which is not PENDING. Status: {booking.Status}");
                    return new Response(StatusCodes.Status400BadRequest, "Only pending bookings can be confirmed", null);
                }
                if (booking.Payment!.Status != PaymentStatus.DRAFT)
                {
                    _logger.LogWarning($"Attempted to confirm booking {id} but its Payment {booking.PaymentId} status is not DRAFT. Status: {booking.Payment.Status}");
                    return new Response(StatusCodes.Status400BadRequest, "Associated payment record is not in the expected DRAFT state.", null);
                }

                // 1: Update local booking status first
                bool statusUpdated = await _bookingDao.UpdateStatusAsync(id, BookingStatus.CONFIRMED);

                if (!statusUpdated)
                {
                    _logger.LogError($"Failed to update status to CONFIRMED for booking {id}.");
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to update booking status", null);
                }
                _logger.LogInformation($"Successfully updated booking {id} status to CONFIRMED.");

                // 2: Trigger Stripe Invoice Creation
                _logger.LogInformation($"Calling StripeService to create invoice for booking {id}.");
                Payment? updatedPayment = await _stripeService.CreateAndSendInvoiceForBookingAsync(id);

                if (updatedPayment == null)
                {
                    // If we got here, then the booking was confirmed locally, but the invoice creation failed.
                    // We could manually send the invoice on Stripe Dashboard, or we could create a trigger to do this.
                    // TODO: Think about a solution for this.
                    _logger.LogError($"Stripe invoice creation failed for confirmed booking {id}. The booking remains confirmed, but the invoice needs attention.");
                }
                else
                {
                    _logger.LogInformation($"Stripe invoice creation initiated successfully for booking {id}. Payment record {updatedPayment.Id} updated.");
                }

                // Get the updated booking to return
                var confirmedBooking = await _bookingDao.GetByIdAsync(id);
                return new Response(StatusCodes.Status200OK, "Booking confirmed successfully. Invoice process initiated.", confirmedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during booking confirmation for ID {id}: {ex.Message}");
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
