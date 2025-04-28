using Microsoft.AspNetCore.Mvc;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Services.BookingService;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        // Constructor to inject BookingService dependency
        public BookingController(BookingDao bookingDao)
        {
            // Create BookingService instance using injected DAO
            this._bookingService = new BookingService(bookingDao);
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<Response>> GetAllBookings()
        {
            Response response = await _bookingService.GetAllBookingsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Response>> GetBookingById(Guid id)
        {
            Response response = await _bookingService.GetBookingByIdAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/renter/{renterId}
        [HttpGet("renter/{renterId}")]
        public async Task<ActionResult<Response>> GetBookingsByRenterId(Guid renterId)
        {
            Response response = await _bookingService.GetBookingsByRenterIdAsync(renterId);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/storage/{storageLocationId}
        [HttpGet("storage/{storageLocationId}")]
        public async Task<ActionResult<Response>> GetBookingsByStorageLocationId(Guid storageLocationId)
        {
            Response response = await _bookingService.GetBookingsByStorageLocationIdAsync(storageLocationId);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<Response>> GetBookingsByStatus(BookingStatus status)
        {
            Response response = await _bookingService.GetBookingsByStatusAsync(status);
            return StatusCode(response.Status, response);
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<Response>> CreateBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = await _bookingService.CreateBookingAsync(booking);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}/confirm
        [HttpPut("{id}/confirm")]
        public async Task<ActionResult<Response>> ConfirmBooking(Guid id, [FromBody] Guid paymentId)
        {
            Response response = await _bookingService.ConfirmBookingAsync(id, paymentId);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<Response>> CancelBooking(Guid id)
        {
            Response response = await _bookingService.CancelBookingAsync(id);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}
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

        // GET: api/bookings/active
        [HttpGet("active")]
        public async Task<ActionResult<Response>> GetActiveBookings()
        {
            Response response = await _bookingService.GetActiveBookingsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/daterange
        [HttpGet("daterange")]
        public async Task<ActionResult<Response>> GetBookingsForDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = await _bookingService.GetBookingsForDateRangeAsync(startDate, endDate);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/availability
        [HttpGet("availability")]
        public async Task<ActionResult<Response>> CheckStorageLocationAvailability(
            [FromQuery] Guid storageLocationId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = await _bookingService.IsStorageLocationAvailableAsync(storageLocationId, startDate, endDate);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/{id}/duration
        [HttpGet("{id}/duration")]
        public async Task<ActionResult<Response>> GetBookingDuration(Guid id)
        {
            Response response = await _bookingService.CalculateBookingDurationAsync(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<Response>> GetUpcomingBookings()
        {
            Response response = await _bookingService.GetUpcomingBookingsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/current
        [HttpGet("current")]
        public async Task<ActionResult<Response>> GetCurrentBookings()
        {
            Response response = await _bookingService.GetCurrentBookingsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/expired
        [HttpGet("expired")]
        public async Task<ActionResult<Response>> GetExpiredBookings()
        {
            Response response = await _bookingService.GetExpiredBookingsAsync();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/starting-within/{days}
        [HttpGet("starting-within/{days}")]
        public async Task<ActionResult<Response>> GetBookingsStartingWithinDays(int days)
        {
            Response response = await _bookingService.GetBookingsStartingWithinDaysAsync(days);
            return StatusCode(response.Status, response);
        }

        // HEAD: api/bookings/{id}
        [HttpHead("{id}")]
        public async Task<IActionResult> CheckBookingExists(Guid id)
        {
            var response = await _bookingService.GetBookingByIdAsync(id);
            return response.Status == 200 ? Ok() : NotFound();
        }
    }
}