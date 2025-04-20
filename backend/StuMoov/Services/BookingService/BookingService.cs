using System.ComponentModel.DataAnnotations;
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
        public Response CreateBooking(Booking booking)
        {
            try
            {
                if (booking == null)
                    return new Response(StatusCodes.Status400BadRequest, "Booking cannot be null", null);

                // Validate the booking
                ValidateBooking(booking);

                // Check if the storage location is available for the requested dates
                if (!_bookingDao.IsStorageLocationAvailable(booking.StorageLocationId, booking.StartDate, booking.EndDate))
                    return new Response(StatusCodes.Status409Conflict, "The storage location is not available for the requested dates", null);

                _bookingDao.CreateBooking(booking);

                return new Response(StatusCodes.Status201Created, "Booking created successfully", booking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get a booking by ID
        public Response GetBookingById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = _bookingDao.GetBookingById(id);
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
        public Response GetAllBookings()
        {
            try
            {
                var bookings = _bookingDao.GetAllBookings();
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by renter ID
        public Response GetBookingsByRenterId(Guid renterId)
        {
            try
            {
                if (renterId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Renter ID cannot be empty", null);

                var bookings = _bookingDao.GetBookingsByRenterId(renterId);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by storage location ID
        public Response GetBookingsByStorageLocationId(Guid storageLocationId)
        {
            try
            {
                if (storageLocationId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Storage location ID cannot be empty", null);

                var bookings = _bookingDao.GetBookingsByStorageLocationId(storageLocationId);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings by status
        public Response GetBookingsByStatus(BookingStatus status)
        {
            try
            {
                var bookings = _bookingDao.GetBookingsByStatus(status);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Confirm a booking
        public Response ConfirmBooking(Guid id, Guid paymentId)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                if (paymentId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Payment ID cannot be empty", null);

                var booking = _bookingDao.GetBookingById(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (booking.Status != BookingStatus.PENDING)
                    return new Response(StatusCodes.Status400BadRequest, "Only pending bookings can be confirmed", null);

                // Set payment ID
                if (!_bookingDao.SetPaymentId(id, paymentId))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to set payment ID", null);

                // Update booking status
                if (!_bookingDao.UpdateBookingStatus(id, BookingStatus.CONFIRMED))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to update booking status", null);

                // Get the updated booking
                var updatedBooking = _bookingDao.GetBookingById(id);
                return new Response(StatusCodes.Status200OK, "Booking confirmed successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Cancel a booking
        public Response CancelBooking(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = _bookingDao.GetBookingById(id);
                if (booking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (booking.Status == BookingStatus.CANCELLED)
                    return new Response(StatusCodes.Status400BadRequest, "Booking is already cancelled", null);

                if (!_bookingDao.UpdateBookingStatus(id, BookingStatus.CANCELLED))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to cancel booking", null);

                // Get the updated booking
                var updatedBooking = _bookingDao.GetBookingById(id);
                return new Response(StatusCodes.Status200OK, "Booking cancelled successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Update booking
        public Response UpdateBooking(Guid id, DateTime startDate, DateTime endDate, decimal totalPrice)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                if (totalPrice <= 0)
                    return new Response(StatusCodes.Status400BadRequest, "Total price must be greater than zero", null);

                var existingBooking = _bookingDao.GetBookingById(id);
                if (existingBooking == null)
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {id} not found", null);

                if (existingBooking.Status == BookingStatus.CANCELLED)
                    return new Response(StatusCodes.Status400BadRequest, "Cannot update a cancelled booking", null);

                // Check if new dates overlap with existing bookings (excluding the current booking)
                var storageLocationId = existingBooking.StorageLocationId;
                var overlappingBookings = _bookingDao.GetBookingsByStorageLocationId(storageLocationId)
                    .Where(b => b.Id != id && b.Status != BookingStatus.CANCELLED)
                    .Where(b =>
                        (b.StartDate >= startDate && b.StartDate < endDate) ||
                        (b.EndDate > startDate && b.EndDate <= endDate) ||
                        (b.StartDate <= startDate && b.EndDate >= endDate)
                    );

                if (overlappingBookings.Any())
                    return new Response(StatusCodes.Status409Conflict, "The requested dates conflict with existing bookings", null);

                // Update the booking
                if (!_bookingDao.UpdateBooking(id, startDate, endDate, totalPrice))
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to update booking", null);

                // Get the updated booking
                var updatedBooking = _bookingDao.GetBookingById(id);
                return new Response(StatusCodes.Status200OK, "Booking updated successfully", updatedBooking);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get active bookings
        public Response GetActiveBookings()
        {
            try
            {
                var bookings = _bookingDao.GetActiveBookings();
                return new Response(StatusCodes.Status200OK, "Active bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Get bookings for a date range
        public Response GetBookingsForDateRange(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                var bookings = _bookingDao.GetBookingsForDateRange(startDate, endDate);
                return new Response(StatusCodes.Status200OK, "Bookings retrieved successfully", bookings);
            }
            catch (Exception ex)
            {
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

        // Check if a storage location is available for a date range
        public Response IsStorageLocationAvailable(Guid storageLocationId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (storageLocationId == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Storage location ID cannot be empty", null);

                if (startDate >= endDate)
                    return new Response(StatusCodes.Status400BadRequest, "Start date must be before end date", null);

                bool isAvailable = _bookingDao.IsStorageLocationAvailable(storageLocationId, startDate, endDate);
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
        public Response CalculateBookingDuration(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty", null);

                var booking = _bookingDao.GetBookingById(id);
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
        public Response GetUpcomingBookings()
        {
            try
            {
                var now = DateTime.Now;
                var bookings = _bookingDao.GetActiveBookings()
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
        public Response GetCurrentBookings()
        {
            try
            {
                var now = DateTime.Now;
                var bookings = _bookingDao.GetActiveBookings()
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
        public Response GetExpiredBookings()
        {
            try
            {
                var now = DateTime.Now;
                var bookings = _bookingDao.GetAllBookings()
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
        public Response GetBookingsStartingWithinDays(int days)
        {
            try
            {
                if (days < 0)
                    return new Response(StatusCodes.Status400BadRequest, "Days must be a non-negative number", null);

                var now = DateTime.Now;
                var futureDate = now.AddDays(days);

                var bookings = _bookingDao.GetActiveBookings()
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
