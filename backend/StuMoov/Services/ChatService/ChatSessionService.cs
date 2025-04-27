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

namespace StuMoov.Services.ChatService
{
    public class ChatSessionService
    {
        private readonly ChatSessionDao _chatSessionDao;
        private readonly UserDao _userDao;
        private readonly BookingDao _bookingDao;
        private readonly ILogger<ChatSessionService> _logger;

        public ChatSessionService(
            ChatSessionDao chatSessionDao,
            UserDao userDao,
            BookingDao bookingDao,
            ILogger<ChatSessionService> logger)
        {
            _chatSessionDao = chatSessionDao;
            _userDao = userDao;
            _bookingDao = bookingDao;
            _logger = logger;
        }

        // Get a chat session by its ID
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

        // Get all chat sessions for a specific user (either as renter or lender)
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

        // Get an existing chat session between a specific renter and lender
        public async Task<Response> GetSessionByParticipantsAsync(Guid renterId, Guid lenderId)
        {
            if (renterId == Guid.Empty || lenderId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be empty.", null);
            }
            if (renterId == lenderId)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be the same.", null);
            }

            try
            {
                // Validate users exist
                User? renterUser = await _userDao.GetUserByIdAsync(renterId);
                User? lenderUser = await _userDao.GetUserByIdAsync(lenderId);

                if (renterUser == null || lenderUser == null)
                {
                    return new Response(StatusCodes.Status404NotFound, "Session not found (one or both users do not exist).", null);
                }

                // Check if a session exists between these two
                List<ChatSession>? existingSessions = await _chatSessionDao.GetByRenterIdAsync(renterId);
                ChatSession? existingSession = existingSessions?.FirstOrDefault(s => s.LenderId == lenderId);

                if (existingSession != null)
                {
                    return new Response(StatusCodes.Status200OK, "Existing chat session retrieved.", existingSession);
                }
                else
                {
                    return new Response(StatusCodes.Status404NotFound, "Chat session between the specified participants not found.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting chat session between Renter {renterId} and Lender {lenderId}.", renterId, lenderId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while getting the chat session.", null);
            }
        }

        // Create a new chat session between a specific renter and lender
        public async Task<Response> CreateSessionAsync(Guid renterId, Guid lenderId)
        {
            if (renterId == Guid.Empty || lenderId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be empty.", null);
            }
            if (renterId == lenderId)
            {
                return new Response(StatusCodes.Status400BadRequest, "Renter ID and Lender ID cannot be the same.", null);
            }

            try
            {
                // Validate both users exist and have the correct roles
                User? renterUser = await _userDao.GetUserByIdAsync(renterId);
                User? lenderUser = await _userDao.GetUserByIdAsync(lenderId);

                if (renterUser == null || lenderUser == null)
                {
                    return new Response(StatusCodes.Status404NotFound, "Renter or Lender not found.", null);
                }

                // Ensure the users have the expected roles
                if (!(renterUser is Renter renter))
                {
                    return new Response(StatusCodes.Status400BadRequest, $"User {renterId} is not a Renter.", null);
                }
                if (!(lenderUser is Lender lender))
                {
                    return new Response(StatusCodes.Status400BadRequest, $"User {lenderId} is not a Lender.", null);
                }

                // Check if a session already exists to prevent duplicates
                List<ChatSession>? existingSessions = await _chatSessionDao.GetByRenterIdAsync(renterId);
                if (existingSessions?.Any(s => s.LenderId == lenderId) ?? false)
                {
                    return new Response(StatusCodes.Status409Conflict, "A chat session already exists between these participants.", null);
                }

                // Create a new session if one doesn't exist
                ChatSession newSession = new ChatSession(renter, lender);
                ChatSession? createdSession = await _chatSessionDao.AddAsync(newSession);

                if (createdSession == null)
                {
                    _logger.LogError("Failed to add new chat session to DB between Renter {RenterId} and Lender {LenderId}", renterId, lenderId);
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to create a new chat session.", null);
                }

                // Refetch to include navigation properties (Renter, Lender)
                ChatSession? detailedSession = await _chatSessionDao.GetByIdAsync(createdSession.Id);

                return new Response(StatusCodes.Status201Created, "New chat session created successfully.", detailedSession ?? createdSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating chat session between Renter {renterId} and Lender {lenderId}.", renterId, lenderId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while creating the chat session.", null);
            }
        }

        // Get chat session by Booking ID
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

        // Associate a Booking with an existing ChatSession
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

                // Check if the session is already associated with a booking
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

                // Fetch the booking (includes Renter and StorageLocation)
                Booking? booking = await _bookingDao.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Booking with ID {bookingId} not found.", null);
                }

                // Validate that the booking parties match the chat session parties
                if (booking.RenterId != session.RenterId || booking.StorageLocation?.LenderId != session.LenderId)
                {
                    _logger.LogWarning("Attempted to associate booking {BookingId} (Renter: {BookingRenterId}, Lender: {BookingLenderId}) with session {SessionId} (Renter: {SessionRenterId}, Lender: {SessionLenderId}) - Mismatch.",
                        bookingId, booking.RenterId, booking.StorageLocation?.LenderId, sessionId, session.RenterId, session.LenderId);
                    return new Response(StatusCodes.Status400BadRequest, "Booking parties do not match chat session parties.", null);
                }

                // Use the helper method on the model to update BookingId and UpdatedAt
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