/**
 * BookingController.cs
 * 
 * Manages storage space booking operations including creation, modification, 
 * cancellation, confirmation, and various query endpoints for booking data.
 * Provides a comprehensive API for the booking lifecycle within the StuMoov platform.
 */

using Microsoft.AspNetCore.Mvc;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Services.BookingService;
using System.ComponentModel.DataAnnotations;
using StuMoov.Models.StorageLocationModel;
using StuMoov.Models.UserModel;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;     // Service handling booking business logic
        private readonly UserDao _userDao;                   // Data access for user entities
        private readonly StorageLocationDao _storageLocationDao; // Data access for storage locations

        /// <summary>
        /// Initializes a new instance of the BookingController
        /// </summary>
        /// <param name="bookingService">Service for booking operations</param>
        /// <param name="userDao">Data access for user operations</param>
        /// <param name="storageLocationDao">Data access for storage location operations</param>
        public BookingController(BookingService bookingService, UserDao userDao, StorageLocationDao storageLocationDao)
        {
            _bookingService = bookingService;
            _userDao = userDao;
            _storageLocationDao = storageLocationDao;
        }

        /// <summary>
        /// Retrieves all bookings in the system
        /// </summary>
        /// <returns>List of all bookings</returns>
        /// <route>GET: api/bookings</route>
        [HttpGet]
        public async Task<ActionResult<Response>> GetAllBookings()
        {
            Response response = await _bookingService.GetAllBookingsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a specific booking by its ID
        /// </summary>
        /// <param name="id">The unique identifier of the booking</param>
        /// <returns>The booking details if found</returns>
        /// <route>GET: api/bookings/{id}</route>
        [HttpGet("{id}")]
        public async Task<ActionResult<Response>> GetBookingById(Guid id)
        {
            Response response = await _bookingService.GetBookingByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all bookings for a specific renter
        /// </summary>
        /// <param name="renterId">The unique identifier of the renter</param>
        /// <returns>List of bookings associated with the specified renter</returns>
        /// <route>GET: api/bookings/renter/{renterId}</route>
        [HttpGet("renter/{renterId}")]
        public async Task<ActionResult<Response>> GetBookingsByRenterId(Guid renterId)
        {
            Response response = await _bookingService.GetBookingsByRenterIdAsync(renterId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all bookings for a specific storage location
        /// </summary>
        /// <param name="storageLocationId">The unique identifier of the storage location</param>
        /// <returns>List of bookings associated with the specified storage location</returns>
        /// <route>GET: api/bookings/storage/{storageLocationId}</route>
        [HttpGet("storage/{storageLocationId}")]
        public async Task<ActionResult<Response>> GetBookingsByStorageLocationId(Guid storageLocationId)
        {
            Response response = await _bookingService.GetBookingsByStorageLocationIdAsync(storageLocationId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all bookings with a specific status
        /// </summary>
        /// <param name="status">The booking status to filter by</param>
        /// <returns>List of bookings with the specified status</returns>
        /// <route>GET: api/bookings/status/{status}</route>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<Response>> GetBookingsByStatus(BookingStatus status)
        {
            Response response = await _bookingService.GetBookingsByStatusAsync(status);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Creates a new booking in the system
        /// </summary>
        /// <param name="request">The booking creation details</param>
        /// <returns>Created booking details with generated ID</returns>
        /// <route>POST: api/bookings</route>
        [HttpPost]
        public async Task<ActionResult<Response>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            // Validate request model
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request data.", ModelState));
            }

            // Fetch related entities to ensure they exist
            User? renterUser = await _userDao.GetUserByIdAsync(request.RenterId);
            StorageLocation? storageLocation = await _storageLocationDao.GetByIdAsync(request.StorageLocationId);

            // Validate fetched objects and ensure proper type casting
            if (renterUser == null || !(renterUser is Renter renter))
            {
                return NotFound(new Response(StatusCodes.Status404NotFound, "Renter not found or user is not a Renter.", null));
            }
            if (storageLocation == null)
            {
                return NotFound(new Response(StatusCodes.Status404NotFound, "Storage location not found.", null));
            }

            // Create booking with validated entities
            Response response = await _bookingService.CreateBookingAsync(request, renter, storageLocation);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Confirms a pending booking
        /// </summary>
        /// <param name="id">The unique identifier of the booking to confirm</param>
        /// <returns>Updated booking details with status changed to confirmed</returns>
        /// <route>PUT: api/bookings/{id}/confirm</route>
        [HttpPut("{id}/confirm")]
        public async Task<ActionResult<Response>> ConfirmBooking(Guid id)
        {
            Response response = await _bookingService.ConfirmBookingAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Cancels a booking
        /// </summary>
        /// <param name="id">The unique identifier of the booking to cancel</param>
        /// <returns>Updated booking details with status changed to canceled</returns>
        /// <route>PUT: api/bookings/{id}/cancel</route>
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<Response>> CancelBooking(Guid id)
        {
            Response response = await _bookingService.CancelBookingAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Updates an existing booking's details
        /// </summary>
        /// <param name="id">The unique identifier of the booking to update</param>
        /// <param name="startDate">New start date for the booking</param>
        /// <param name="endDate">New end date for the booking</param>
        /// <param name="totalPrice">New total price for the booking</param>
        /// <returns>Updated booking details</returns>
        /// <route>PUT: api/bookings/{id}</route>
        [HttpPut("{id}")]
        public async Task<ActionResult<Response>> UpdateBooking(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] decimal totalPrice)
        {
            Response response = await _bookingService.UpdateBookingAsync(id, startDate, endDate, totalPrice);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all active bookings (not canceled or completed)
        /// </summary>
        /// <returns>List of all active bookings</returns>
        /// <route>GET: api/bookings/active</route>
        [HttpGet("active")]
        public async Task<ActionResult<Response>> GetActiveBookings()
        {
            Response response = await _bookingService.GetActiveBookingsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all bookings within a specified date range
        /// </summary>
        /// <param name="startDate">Start date of the range</param>
        /// <param name="endDate">End date of the range</param>
        /// <returns>List of bookings that overlap with the specified date range</returns>
        /// <route>GET: api/bookings/daterange?startDate={startDate}&endDate={endDate}</route>
        [HttpGet("daterange")]
        public async Task<ActionResult<Response>> GetBookingsForDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = await _bookingService.GetBookingsForDateRangeAsync(startDate, endDate);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Checks if a storage location is available during a specified date range
        /// </summary>
        /// <param name="storageLocationId">The unique identifier of the storage location</param>
        /// <param name="startDate">Start date of the period to check</param>
        /// <param name="endDate">End date of the period to check</param>
        /// <returns>Availability status of the storage location</returns>
        /// <route>GET: api/bookings/availability?storageLocationId={id}&startDate={start}&endDate={end}</route>
        [HttpGet("availability")]
        public async Task<ActionResult<Response>> CheckStorageLocationAvailability(
            [FromQuery] Guid storageLocationId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = await _bookingService.IsStorageLocationAvailableAsync(storageLocationId, startDate, endDate);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Calculates the duration of a specific booking in days
        /// </summary>
        /// <param name="id">The unique identifier of the booking</param>
        /// <returns>Duration of the booking in days</returns>
        /// <route>GET: api/bookings/{id}/duration</route>
        [HttpGet("{id}/duration")]
        public async Task<ActionResult<Response>> GetBookingDuration(Guid id)
        {
            Response response = await _bookingService.CalculateBookingDurationAsync(id);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all upcoming bookings (future start date)
        /// </summary>
        /// <returns>List of upcoming bookings</returns>
        /// <route>GET: api/bookings/upcoming</route>
        [HttpGet("upcoming")]
        public async Task<ActionResult<Response>> GetUpcomingBookings()
        {
            Response response = await _bookingService.GetUpcomingBookingsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all currently active bookings (between start and end date)
        /// </summary>
        /// <returns>List of current bookings</returns>
        /// <route>GET: api/bookings/current</route>
        [HttpGet("current")]
        public async Task<ActionResult<Response>> GetCurrentBookings()
        {
            Response response = await _bookingService.GetCurrentBookingsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all expired bookings (past end date)
        /// </summary>
        /// <returns>List of expired bookings</returns>
        /// <route>GET: api/bookings/expired</route>
        [HttpGet("expired")]
        public async Task<ActionResult<Response>> GetExpiredBookings()
        {
            Response response = await _bookingService.GetExpiredBookingsAsync();
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves bookings that are starting within a specified number of days
        /// </summary>
        /// <param name="days">Number of days from current date to check</param>
        /// <returns>List of bookings starting within the specified days</returns>
        /// <route>GET: api/bookings/starting-within/{days}</route>
        [HttpGet("starting-within/{days}")]
        public async Task<ActionResult<Response>> GetBookingsStartingWithinDays(int days)
        {
            Response response = await _bookingService.GetBookingsStartingWithinDaysAsync(days);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Checks if a booking with the specified ID exists
        /// </summary>
        /// <param name="id">The unique identifier of the booking to check</param>
        /// <returns>200 OK if booking exists, 404 Not Found otherwise</returns>
        /// <route>HEAD: api/bookings/{id}</route>
        [HttpHead("{id}")]
        public async Task<IActionResult> CheckBookingExists(Guid id)
        {
            var response = await _bookingService.GetBookingByIdAsync(id);
            return response.Status == 200 ? Ok() : NotFound();
        }
    }

    /// <summary>
    /// Data transfer object for creating a new booking
    /// </summary>
    public class CreateBookingRequest
    {
        /// <summary>
        /// Unique identifier of the renter making the booking
        /// </summary>
        [Required]
        public Guid RenterId { get; set; }

        /// <summary>
        /// Unique identifier of the storage location being booked
        /// </summary>
        [Required]
        public Guid StorageLocationId { get; set; }

        /// <summary>
        /// Start date of the booking period
        /// </summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the booking period
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total price for the booking duration in cents
        /// </summary>
        [Required]
        public decimal TotalPrice { get; set; } // Price in cents
    }
}