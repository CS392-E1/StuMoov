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

        // Constructor to inject BookingDao dependency
        public BookingController(BookingDao bookingDao)
        {
            // Create BookingService instance using injected DAO
            this._bookingService = new BookingService(bookingDao);
        }

        // GET: api/bookings
        [HttpGet]
        public ActionResult<Response> GetAllBookings()
        {
            Response response = _bookingService.GetAllBookings();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/{id}
        [HttpGet("{id}")]
        public ActionResult<Response> GetBookingById(Guid id)
        {
            Response response = _bookingService.GetBookingById(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/renter/{renterId}
        [HttpGet("renter/{renterId}")]
        public ActionResult<Response> GetBookingsByRenterId(Guid renterId)
        {
            Response response = _bookingService.GetBookingsByRenterId(renterId);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/storage/{storageLocationId}
        [HttpGet("storage/{storageLocationId}")]
        public ActionResult<Response> GetBookingsByStorageLocationId(Guid storageLocationId)
        {
            Response response = _bookingService.GetBookingsByStorageLocationId(storageLocationId);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/status/{status}
        [HttpGet("status/{status}")]
        public ActionResult<Response> GetBookingsByStatus(BookingStatus status)
        {
            Response response = _bookingService.GetBookingsByStatus(status);
            return StatusCode(response.Status, response);
        }

        // POST: api/bookings
        [HttpPost]
        public ActionResult<Response> CreateBooking([FromBody] Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Response response = _bookingService.CreateBooking(booking);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}/confirm
        [HttpPut("{id}/confirm")]
        public ActionResult<Response> ConfirmBooking(Guid id, [FromBody] Guid paymentId)
        {
            Response response = _bookingService.ConfirmBooking(id, paymentId);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}/cancel
        [HttpPut("{id}/cancel")]
        public ActionResult<Response> CancelBooking(Guid id)
        {
            Response response = _bookingService.CancelBooking(id);
            return StatusCode(response.Status, response);
        }

        // PUT: api/bookings/{id}
        [HttpPut("{id}")]
        public ActionResult<Response> UpdateBooking(
            Guid id,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] decimal totalPrice)
        {
            Response response = _bookingService.UpdateBooking(id, startDate, endDate, totalPrice);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/active
        [HttpGet("active")]
        public ActionResult<Response> GetActiveBookings()
        {
            Response response = _bookingService.GetActiveBookings();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/daterange
        [HttpGet("daterange")]
        public ActionResult<Response> GetBookingsForDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = _bookingService.GetBookingsForDateRange(startDate, endDate);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/availability
        [HttpGet("availability")]
        public ActionResult<Response> CheckStorageLocationAvailability(
            [FromQuery] Guid storageLocationId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            Response response = _bookingService.IsStorageLocationAvailable(storageLocationId, startDate, endDate);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/{id}/duration
        [HttpGet("{id}/duration")]
        public ActionResult<Response> GetBookingDuration(Guid id)
        {
            Response response = _bookingService.CalculateBookingDuration(id);
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/upcoming
        [HttpGet("upcoming")]
        public ActionResult<Response> GetUpcomingBookings()
        {
            Response response = _bookingService.GetUpcomingBookings();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/current
        [HttpGet("current")]
        public ActionResult<Response> GetCurrentBookings()
        {
            Response response = _bookingService.GetCurrentBookings();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/expired
        [HttpGet("expired")]
        public ActionResult<Response> GetExpiredBookings()
        {
            Response response = _bookingService.GetExpiredBookings();
            return StatusCode(response.Status, response);
        }

        // GET: api/bookings/starting-within/{days}
        [HttpGet("starting-within/{days}")]
        public ActionResult<Response> GetBookingsStartingWithinDays(int days)
        {
            Response response = _bookingService.GetBookingsStartingWithinDays(days);
            return StatusCode(response.Status, response);
        }

        // HEAD: api/bookings/{id}
        [HttpHead("{id}")]
        public IActionResult CheckBookingExists(Guid id)
        {
            var response = _bookingService.GetBookingById(id);
            return response.Status == 200 ? Ok() : NotFound();
        }
    }
}