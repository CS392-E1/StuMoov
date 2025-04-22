using StuMoov.Models.BookingModel;

namespace StuMoov.Dao
{
    public class BookingDao
    {
        private Dictionary<Guid, Booking> Bookings;
        public BookingDao() {
            Bookings = new Dictionary<Guid, Booking>();

            Booking booking1 = new Booking(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.MaxValue,
                100.59m
                );

            Booking booking2 = new Booking(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.MaxValue,
                120.00m
                );

            Booking booking3 = new Booking(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.Now,
                DateTime.MaxValue,
                200.00m
                );

            Bookings[booking1.Id] = booking1;
            Bookings[booking2.Id] = booking2;
            Bookings[booking3.Id] = booking3;
        }

        // Create a new booking
        public Booking CreateBooking(Booking newBooking)
        {
            Bookings[newBooking.Id] = newBooking;
            return newBooking;
        }

        // Get a booking by ID
        public Booking? GetBookingById(Guid id)
        {
            return Bookings.TryGetValue(id, out Booking? booking) ? booking : null;
        }

        // Get all bookings
        public List<Booking> GetAllBookings()
        {
            return Bookings.Values.ToList();
        }

        // Get bookings by renter ID
        public List<Booking> GetBookingsByRenterId(Guid renterId)
        {
            return Bookings.Values.Where(b => b.RenterId == renterId).ToList();
        }

        // Get bookings by storage location ID
        public List<Booking> GetBookingsByStorageLocationId(Guid storageLocationId)
        {
            return Bookings.Values.Where(b => b.StorageLocationId == storageLocationId).ToList();
        }

        // Get bookings by status
        public List<Booking> GetBookingsByStatus(BookingStatus status)
        {
            return Bookings.Values.Where(b => b.Status == status).ToList();
        }

        // Update booking status
        public bool UpdateBookingStatus(Guid id, BookingStatus newStatus)
        {
            if (Bookings.TryGetValue(id, out Booking? booking))
            {
                booking.Status = newStatus;
                return true;
            }
            return false;
        }

        // Update booking details
        public bool UpdateBooking(Guid id, DateTime startDate, DateTime endDate, decimal totalPrice)
        {
            if (Bookings.TryGetValue(id, out Booking? booking))
            {
                booking.StartDate = startDate;
                booking.EndDate = endDate;
                booking.TotalPrice = totalPrice;
                return true;
            }
            return false;
        }

        // Set payment ID for a booking
        public bool SetPaymentId(Guid bookingId, Guid paymentId)
        {
            if (Bookings.TryGetValue(bookingId, out Booking? booking))
            {
                // Use reflection to set the private property
                var property = typeof(Booking).GetProperty("PaymentId");
                property?.SetValue(booking, paymentId);
                return true;
            }
            return false;
        }

        // Delete a booking
        public bool DeleteBooking(Guid id)
        {
            return Bookings.Remove(id);
        }

        // Get active bookings (not cancelled)
        public List<Booking> GetActiveBookings()
        {
            return Bookings.Values.Where(b => b.Status != BookingStatus.CANCELLED).ToList();
        }

        // Get bookings for a date range
        public List<Booking> GetBookingsForDateRange(DateTime startDate, DateTime endDate)
        {
            return Bookings.Values.Where(b =>
                (b.StartDate >= startDate && b.StartDate <= endDate) ||
                (b.EndDate >= startDate && b.EndDate <= endDate) ||
                (b.StartDate <= startDate && b.EndDate >= endDate)
            ).ToList();
        }

        // Check if a storage location is available for a date range
        public bool IsStorageLocationAvailable(Guid storageLocationId, DateTime startDate, DateTime endDate)
        {
            var conflictingBookings = Bookings.Values.Where(b =>
                b.StorageLocationId == storageLocationId &&
                b.Status != BookingStatus.CANCELLED &&
                ((b.StartDate >= startDate && b.StartDate < endDate) ||
                 (b.EndDate > startDate && b.EndDate <= endDate) ||
                 (b.StartDate <= startDate && b.EndDate >= endDate))
            );

            return !conflictingBookings.Any();
        }
    }
}
