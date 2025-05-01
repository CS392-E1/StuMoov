/**
 * ChatSessionService.cs
 *
 * Manages chat session operations for the StuMoov application.
 * Provides functionality to create, retrieve, and associate chat sessions between renters and lenders
 * for specific storage locations or bookings. Integrates with DAOs for data access and logging for diagnostics.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.BookingModel;
using StuMoov.Models.ChatModel;
using StuMoov.Models.UserModel;
using StuMoov.Models.StorageLocationModel;

namespace StuMoov.Services.ChatService
{
    /// <summary>
    /// Service responsible for managing chat session operations, including creation, retrieval,
    /// and association with bookings. Ensures validation of participants and storage locations.
    /// </summary>
    public class ChatSessionService
    {
        /// <summary>
        /// Data access object for chat session-related database operations.
        /// </summary>
        private readonly ChatSessionDao _chatSessionDao;

        /// <summary>
        /// Data access object for user-related database operations.
        /// </summary>
        private readonly UserDao _userDao;

        /// <summary>
        /// Data access object for booking-related database operations.
        /// </summary>
        private readonly BookingDao _bookingDao;

        /// <summary>
        /// Data access object for storage location-related database operations.
        /// </summary>
        private readonly StorageLocationDao _storageLocationDao;

        /// <summary>
        /// Logger for recording chat session-related events and errors.
        /// </summary>
        private readonly ILogger<ChatSessionService> _logger;

        /// <summary>
        /// Initializes a new instance of the ChatSessionService with required dependencies.
        /// </summary>
        /// <param name="chatSessionDao">DAO for chat session operations.</param>
        /// <param name="userDao">DAO for user operations.</param>
        /// <param name="bookingDao">DAO for booking operations.</param>
        /// <param name="storageLocationDao">DAO for storage location operations.</param>
        /// <param name="logger">Logger for diagnostics.</param>
        public ChatSessionService(
            ChatSessionDao chatSessionDao,
            UserDao userDao,
            BookingDao bookingDao,
            StorageLocationDao storageLocationDao,
            ILogger<ChatSessionService> logger)
        {
            _chatSessionDao = chatSessionDao;
            _userDao = userDao;
            _bookingDao = bookingDao;
            _storageLocationDao = storageLocationDao;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a chat session by its unique identifier.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <returns>A Response object with the status, message, and chat session data.</returns>
        public async Task<Response> GetSessionByIdAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Session ID cannot be empty.", null);
            }

            try
            {
                ChatSession? session = await _chatSessionDao.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Chat session with ID {sessionId} not found.", null);
                }
                return new Response(StatusCodes.Status200OK, "Chat session retrieved successfully.", session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chat session with ID {sessionId}.");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the chat session.", null);
            }
        }

        /// <summary>
        /// Retrieves all chat sessions for a specific user, where they are either the renter or lender.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A Response object with the status, message, and list of chat sessions.</returns>
        public async Task<Response> GetSessionsByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "User ID cannot be empty.", null);
            }

            try
            {
                // Optional: Check if user exists
                User? user = await _userDao.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"User with ID {userId} not found.", null);
                }

                List<ChatSession>? sessions = await _chatSessionDao.GetByUserIdAsync(userId);
                return new Response(StatusCodes.Status200OK, "Chat sessions retrieved successfully.", sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chat sessions for user ID {userId}.");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while retrieving chat sessions.", null);
            }
        }

        /// <summary>
        /// Retrieves an existing chat session between a renter, lender, and for a specific storage location.
        /// </summary>
        /// <param name="renterId">The ID of the renter.</param>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <param name="storageLocationId">The ID of the storage location.</param>
        /// <returns>A Response object with the status, message, and existing chat session, if found.</returns>
        public async Task<Response> GetSessionByParticipantsAsync(
            Guid renterId,
            Guid lenderId,
            Guid storageLocationId)
        {
            if (renterId == Guid.Empty || lenderId == Guid.Empty || storageLocationId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID, Lender ID, and Storage Location ID cannot be empty.", null);
            }
            if (renterId == lenderId)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be the same.", null);
            }

            try
            {
                ChatSession? existingSession = await _chatSessionDao.GetByParticipantsAndListingAsync(
                    renterId,
                    lenderId,
                    storageLocationId
                );

                if (existingSession != null)
                {
                    return new Response(StatusCodes.Status200OK, "Existing chat session retrieved.", existingSession);
                }
                else
                {
                    return new Response(StatusCodes.Status404NotFound, "Chat session between the specified participants for this listing not found.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat session between Renter {renterId}, Lender {lenderId}, and Listing {storageLocationId}.", renterId, lenderId, storageLocationId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while getting the chat session.", null);
            }
        }

        /// <summary>
        /// Creates a new chat session between a renter, lender, and for a specific storage location.
        /// Returns an existing session if one already exists for the same participants and listing.
        /// </summary>
        /// <param name="renterId">The ID of the renter.</param>
        /// <param name="lenderId">The ID of the lender.</param>
        /// <param name="storageLocationId">The ID of the storage location.</param>
        /// <returns>A Response object with the status, message, and created or existing chat session.</returns>
        public async Task<Response> CreateSessionAsync(
            Guid renterId,
            Guid lenderId,
            Guid storageLocationId)
        {
            if (renterId == Guid.Empty || lenderId == Guid.Empty || storageLocationId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID, Lender ID, and Storage Location ID cannot be empty.", null);
            }
            if (renterId == lenderId)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be the same.", null);
            }

            try
            {
                // Validate renter and lender
                User? renterUser = await _userDao.GetUserByIdAsync(renterId);
                User? lenderUser = await _userDao.GetUserByIdAsync(lenderId);

                if (renterUser == null || lenderUser == null)
                {
                    return new Response(StatusCodes.Status404NotFound, "Renter or Lender not found.", null);
                }
                if (!(renterUser is Renter renter))
                {
                    return new Response(StatusCodes.Status400BadRequest, $"User {renterId} is not a Renter.", null);
                }
                if (!(lenderUser is Lender lender))
                {
                    return new Response(StatusCodes.Status400BadRequest, $"User {lenderId} is not a Lender.", null);
                }

                // Validate storage location
                StorageLocation? storageLocation = await _storageLocationDao.GetByIdAsync(storageLocationId);
                if (storageLocation == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Storage Location with ID {storageLocationId} not found.", null);
                }
                if (storageLocation.LenderId != lenderId)
                {
                    return new Response(StatusCodes.Status400BadRequest, $"Storage Location does not belong to the specified Lender.", null);
                }

                // Check for existing session
                ChatSession? existingSession = await _chatSessionDao.GetByParticipantsAndListingAsync(
                    renterId,
                    lenderId,
                    storageLocationId
                );

                if (existingSession != null)
                {
                    // Return existing session if found
                    return new Response(StatusCodes.Status200OK, "Chat session already exists.", existingSession);
                }

                // Create new session
                ChatSession newSession = new ChatSession(renter, lender, storageLocation);
                ChatSession? createdSession = await _chatSessionDao.AddAsync(newSession);

                if (createdSession == null)
                {
                    _logger.LogError("Failed to add new chat session to DB between Renter {RenterId}, Lender {LenderId} for Listing {StorageLocationId}", renterId, lenderId, storageLocationId);
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to create a new chat session.", null);
                }

                ChatSession? detailedSession = await _chatSessionDao.GetByIdAsync(createdSession.Id);

                return new Response(StatusCodes.Status201Created, "New chat session created successfully.", detailedSession ?? createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating chat session between Renter {renterId}, Lender {lenderId} for Listing {storageLocationId}.", renterId, lenderId, storageLocationId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the chat session.", null);
            }
        }

        /// <summary>
        /// Retrieves a chat session associated with a specific booking.
        /// </summary>
        /// <param name="bookingId">The ID of the booking.</param>
        /// <returns>A Response object with the status, message, and chat session data.</returns>
        public async Task<Response> GetSessionByBookingIdAsync(Guid bookingId)
        {
            if (bookingId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Booking ID cannot be empty.", null);
            }

            try
            {
                ChatSession? session = await _chatSessionDao.GetByBookingIdAsync(bookingId);
                if (session == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Chat session associated with booking ID {bookingId} not found.", null);
                }
                return new Response(StatusCodes.Status200OK, "Chat session retrieved successfully.", session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving chat session for booking ID {bookingId}.");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the chat session.", null);
            }
        }

        /// <summary>
        /// Associates a booking with an existing chat session.
        /// Validates that the booking parties match the session participants.
        /// </summary>
        /// <param name="sessionId">The ID of the chat session.</param>
        /// <param name="bookingId">The ID of the booking to associate.</param>
        /// <returns>A Response object with the status, message, and updated chat session.</returns>
        public async Task<Response> AssociateBookingWithSessionAsync(Guid sessionId, Guid bookingId)
        {
            if (sessionId == Guid.Empty || bookingId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Session ID and Booking ID cannot be empty.", null);
            }

            try
            {
                // Fetch the chat session
                ChatSession? session = await _chatSessionDao.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Chat session with ID {sessionId} not found.", null);
                }

                // Check for existing booking association
                if (session.BookingId.HasValue)
                {
                    // Decide behaviour: error out, or allow changing?
                    // For now, let's prevent changing an existing association.
                    if (session.BookingId.Value == bookingId)
                    {
                        return new Response(StatusCodes.Status200OK, "Session already associated with this booking.", session);
                    }
                    return new Response(StatusCodes.Status409Conflict, $"Chat session is already associated with booking ID {session.BookingId.Value}.", null);
                }

                // Fetch the booking
                Booking? booking = await _bookingDao.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {bookingId} not found.", null);
                }

                // Validate participant match
                if (booking.RenterId != session.RenterId || booking.StorageLocation?.LenderId != session.LenderId)
                {
                    _logger.LogWarning("Attempted to associate booking {BookingId} (Renter: {BookingRenterId}, Lender: {BookingLenderId}) with session {SessionId} (Renter: {SessionRenterId}, Lender: {SessionLenderId}) - Mismatch.",
                        bookingId, booking.RenterId, booking.StorageLocation?.LenderId, sessionId, session.RenterId, session.LenderId);
                    return new Response(StatusCodes.Status400BadRequest, "Booking parties do not match chat session parties.", null);
                }

                // Update session with booking
                session.SetBooking(booking);

                // Update the session in the database using the modified DAO method
                ChatSession? updatedSession = await _chatSessionDao.UpdateAsync(session);

                if (updatedSession == null)
                {
                    _logger.LogError("Failed to update chat session {SessionId} with booking {BookingId} in DB.", sessionId, bookingId);
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to associate booking with chat session.", null);
                }

                return new Response(StatusCodes.Status200OK, "Booking associated with chat session successfully.", updatedSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error associating booking {BookingId} with session {SessionId}.", bookingId, sessionId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while associating the booking.", null);
            }
        }
    }
}