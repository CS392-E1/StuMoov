/**
 * BookingService.cs
 *
 * Manages booking-related operations for the StuMoov application.
 * Provides functionality for creating, retrieving, updating, confirming, and canceling bookings,
 * as well as handling associated payment records and Stripe invoice creation.
 * Integrates with DAOs for data access, Stripe services for payments, and logging for diagnostics.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StuMoov.Controllers;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Models.PaymentModel;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel;

namespace StuMoov.Services.BookingService
{
    /// <summary>
    /// Service responsible for managing booking operations, including creation, retrieval, updates, and cancellations.
    /// Handles payment record creation and Stripe invoice processing for confirmed bookings.
    /// </summary>
    public class BookingService
    {
        /// <summary>
        /// Data access object for booking-related database operations.
        /// </summary>
        [Required]
        private readonly BookingDao _bookingDao;

        /// <summary>
        /// Data access object for payment-related database operations.
        /// </summary>
        private readonly PaymentDao _paymentDao;

        /// <summary>
        /// Data access object for user-related database operations.
        /// </summary>
        private readonly UserDao _userDao;

        /// <summary>
        /// Stripe service for handling payment and invoice operations.
        /// </summary>
        private readonly StuMoov.Services.StripeService.StripeService _stripeService;

        /// <summary>
        /// Configuration for accessing application settings, such as Stripe fees.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Logger for recording booking-related events and errors.
        /// </summary>
        private readonly ILogger<BookingService> _logger;

        /// <summary>
        /// Initializes a new instance of the BookingService with required dependencies.
        /// </summary>
        /// <param name="bookingDao">DAO for booking operations.</param>
        /// <param name="paymentDao">DAO for payment operations.</param>
        /// <param name="userDao">DAO for user operations.</param>
        /// <param name="stripeService">Service for Stripe payment processing.</param>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public BookingService(
            BookingDao bookingDao,
            PaymentDao paymentDao,
            UserDao userDao,
            StuMoov.Services.StripeService.StripeService stripeService,
            IConfiguration configuration,
            ILogger<BookingService> logger)
        {
            _bookingDao = bookingDao;
            _paymentDao = paymentDao;
            _userDao = userDao;
            _stripeService = stripeService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new booking for a storage location by a renter.
        /// Validates inputs, checks availability, creates a payment record, and links it to the booking.
        /// </summary>
        /// <param name="request">The booking creation request with details like dates and price.</param>
        /// <param name="renter">The renter making the booking.</param>
        /// <param name="storageLocation">The storage location being booked.</param>
        /// <returns>A Response object with the status, message, and created booking.</returns>
        public async Task<Response> CreateBookingAsync(CreateBookingRequest request, Renter renter, StorageLocation storageLocation)
        {
            try
            {
                // Validate inputs
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

                // Check availability
                if (!await _bookingDao.IsStorageLocationAvailableAsync(storageLocation.Id, request.StartDate, request.EndDate))
                    return new Response(StatusCodes.Status409Conflict, "The storage location is not available for the requested dates", null);

                // Fetch the Lender associated with the StorageLocation
                Lender? lender = await _userDao.GetUserByIdAsync(storageLocation.LenderId) as Lender;
                if (lender == null)
                {
                    _logger.LogError($"Lender not found for StorageLocation ID: {storageLocation.Id}. Cannot create booking.");
                    return new Response(StatusCodes.Status500InternalServerError, "Could not find the owner of the storage location.", null);
                }

                var now = DateTime.UtcNow;
                Booking newBooking = new Booking
                {
                    RenterId = renter.Id,
                    StorageLocationId = storageLocation.Id,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalPrice = request.TotalPrice,
                    Status = BookingStatus.PENDING,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                Guid? createdBookingId = await _bookingDao.CreateAsync(newBooking);
                if (createdBookingId == null || createdBookingId == Guid.Empty)
                {
                    _logger.LogError("Failed to create booking record in the database.");
                    return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the booking.", null);
                }

                Booking? createdBooking = await _bookingDao.GetByIdAsync(createdBookingId.Value);
                if (createdBooking == null)
                {
                    _logger.LogError($"Failed to retrieve booking record with ID {createdBookingId.Value} after creation.");
                    return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the booking.", null);
                }

                decimal amountChargedInCents = request.TotalPrice;
                decimal applicationFeePercent = _configuration.GetValue<decimal>("Stripe:ApplicationFeePercent", 3m); // Default to 3%
                decimal platformFeeInCents = Math.Round(amountChargedInCents * (applicationFeePercent / 100m), 2);
                decimal amountTransferredInCents = amountChargedInCents - platformFeeInCents;

                _logger.LogInformation($"Initial Payment calculation for Booking {createdBooking.Id}: Total={amountChargedInCents}c, Fee={platformFeeInCents}c, Transferred={amountTransferredInCents}c");

                Payment payment = new Payment(
                    booking: createdBooking,
                    renter: renter,
                    lender: lender,
                    stripeInvoiceId: null,
                    stripePaymentIntentId: string.Empty,
                    amountCharged: amountChargedInCents,
                    platformFee: platformFeeInCents,
                    amountTransferred: amountTransferredInCents
                );

                Payment? createdPayment = await _paymentDao.AddAsync(payment);
                if (createdPayment == null)
                {
                    _logger.LogError($"Failed to create payment record for Booking ID: {createdBooking.Id}. Potential data inconsistency.");
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to create associated payment record.", null);
                }

                createdBooking.PaymentId = createdPayment.Id;
                bool updateSuccess = await _bookingDao.UpdateAsync(createdBooking);
                if (!updateSuccess)
                {
                    _logger.LogError($"Failed to link Payment ID {createdPayment.Id} to Booking ID {createdBooking.Id}.");
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to finalize booking creation.", null);
                }

                _logger.LogInformation($"Booking {createdBooking.Id} and Payment {createdPayment.Id} created successfully.");
                return new Response(StatusCodes.Status201Created, "Booking created successfully", createdBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating booking: {ex.Message}");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the booking.", null);
            }
        }

        /// <summary>
        /// Retrieves a booking by its unique identifier.
        /// </summary>
        /// <param name="id">The ID of the booking to retrieve.</param>
        /// <returns>A Response object with the status, message, and booking data.</returns>
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

        /// <summary>
        /// Retrieves all bookings from the database.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Retrieves all bookings associated with a specific renter.
        /// </summary>
        /// <param name="renterId">The ID of the renter.</param>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Retrieves all bookings associated with a specific storage location.
        /// </summary>
        /// <param name="storageLocationId">The ID of the storage location.</param>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Retrieves all bookings with a specific status.
        /// </summary>
        /// <param name="status">The booking status to filter by.</param>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Confirms a booking and initiates Stripe invoice creation.
        /// Updates the booking status to CONFIRMED and triggers payment processing.
        /// </summary>
        /// <param name="id">The ID of the booking to confirm.</param>
        /// <returns>A Response object with the status, message, and updated booking.</returns>
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
                    _logger.LogError($"Stripe invoice creation failed for confirmed booking {id}. The booking remains confirmed, but the invoice needs attention.");
                }
                else
                {
                    _logger.LogInformation($"Stripe invoice creation initiated successfully for booking {id}. Payment record {updatedPayment.Id} updated.");
                }

                // Get the updated booking
                var confirmedBooking = await _bookingDao.GetByIdAsync(id);
                return new Response(StatusCodes.Status200OK, "Booking confirmed successfully. Invoice process initiated.", confirmedBooking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during booking confirmation for ID {id}: {ex.Message}");
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        /// <summary>
        /// Cancels a booking by updating its status to CANCELLED.
        /// </summary>
        /// <param name="id">The ID of the booking to cancel.</param>
        /// <returns>A Response object with the status, message, and updated booking.</returns>
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

        /// <summary>
        /// Updates an existing booking with new dates and price.
        /// Validates for conflicts with other bookings and ensures the booking is not cancelled.
        /// </summary>
        /// <param name="id">The ID of the booking to update.</param>
        /// <param name="startDate">The new start date for the booking.</param>
        /// <param name="endDate">The new end date for the booking.</param>
        /// <param name="totalPrice">The new total price for the booking.</param>
        /// <returns>A Response object with the status, message, and updated booking.</returns>
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

                // Check for conflicts with other bookings
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

                // Update booking details
                existingBooking.StartDate = startDate;
                existingBooking.EndDate = endDate;
                existingBooking.TotalPrice = totalPrice;
                existingBooking.UpdatedAt = DateTime.UtcNow;

                // Update the booking using the modified object
                if (!await _bookingDao.UpdateAsync(existingBooking))
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

        /// <summary>
        /// Retrieves all active bookings (not cancelled or expired).
        /// </summary>
        /// <returns>A Response object with the status, message, and list of active bookings.</returns>
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

        /// <summary>
        /// Retrieves bookings within a specified date range.
        /// </summary>
        /// <param name="startDate">The start of the date range.</param>
        /// <param name="endDate">The end of the date range.</param>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Checks if a storage location is available for a specified date range.
        /// </summary>
        /// <param name="storageLocationId">The ID of the storage location.</param>
        /// <param name="startDate">The start of the date range.</param>
        /// <param name="endDate">The end of the date range.</param>
        /// <returns>A Response object with the status, message, and availability status.</returns>
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

        /// <summary>
        /// Calculates the duration of a booking in days.
        /// </summary>
        /// <param name="id">The ID of the booking.</param>
        /// <returns>A Response object with the status, message, and duration in days.</returns>
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

        /// <summary>
        /// Retrieves all upcoming bookings with a start date in the future.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of upcoming bookings.</returns>
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

        /// <summary>
        /// Retrieves all current bookings that are active within the current date.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of current bookings.</returns>
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

        /// <summary>
        /// Retrieves all expired bookings with an end date in the past.
        /// </summary>
        /// <returns>A Response object with the status, message, and list of expired bookings.</returns>
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

        /// <summary>
        /// Retrieves bookings starting within a specified number of days from the current date.
        /// </summary>
        /// <param name="days">The number of days to look ahead.</param>
        /// <returns>A Response object with the status, message, and list of bookings.</returns>
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

        /// <summary>
        /// Validates a booking's properties to ensure they meet requirements.
        /// </summary>
        /// <param name="booking">The booking to validate.</param>
        /// <returns>A Response object indicating the validation result.</returns>
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