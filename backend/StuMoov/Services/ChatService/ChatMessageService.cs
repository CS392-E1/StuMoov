using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StuMoov.Dao;
using StuMoov.Models;
using StuMoov.Models.ChatModel;
using StuMoov.Models.UserModel;

namespace StuMoov.Services.ChatService
{
    public class ChatMessageService
    {
        private readonly ChatMessageDao _chatMessageDao;
        private readonly ChatSessionDao _chatSessionDao;
        private readonly UserDao _userDao;
        private readonly ILogger<ChatMessageService> _logger;

        public ChatMessageService(
            ChatMessageDao chatMessageDao,
            ChatSessionDao chatSessionDao,
            UserDao userDao,
            ILogger<ChatMessageService> logger)
        {
            _chatMessageDao = chatMessageDao;
            _chatSessionDao = chatSessionDao;
            _userDao = userDao;
            _logger = logger;
        }

        // Get all messages for a specific chat session
        public async Task<Response> GetMessagesBySessionIdAsync(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Session ID cannot be empty.", null);
            }

            try
            {
                // Verify the session exists first
                var session = await _chatSessionDao.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Chat session with ID {sessionId} not found.", null);
                }

                // Fetch messages for the session
                var messages = await _chatMessageDao.GetBySessionIdAsync(sessionId);
                return new Response(StatusCodes.Status200OK, "Messages retrieved successfully.", messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for session ID {sessionId}.");
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while retrieving messages.", null);
            }
        }

        // Add a new message to a chat session
        public async Task<Response> AddMessageAsync(Guid sessionId, Guid senderId, string content)
        {
            if (sessionId == Guid.Empty || senderId == Guid.Empty)
            {
                return new Response(StatusCodes.Status400BadRequest, "Session ID and Sender ID cannot be empty.", null);
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                return new Response(StatusCodes.Status400BadRequest, "Message content cannot be empty.", null);
            }

            try
            {
                // Validate session exists
                var session = await _chatSessionDao.GetByIdAsync(sessionId);
                if (session == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Chat session with ID {sessionId} not found.", null);
                }

                // Validate sender exists
                var sender = await _userDao.GetUserByIdAsync(senderId);
                if (sender == null)
                {
                    return new Response(StatusCodes.Status404NotFound, $"Sender with ID {senderId} not found.", null);
                }

                // Validate sender is part of the chat session
                if (session.RenterId != senderId && session.LenderId != senderId)
                {
                    _logger.LogWarning("Attempt by user {SenderId} to send message to session {SessionId} they are not part of (Renter: {RenterId}, Lender: {LenderId}).",
                       senderId, sessionId, session.RenterId, session.LenderId);
                    return new Response(StatusCodes.Status403Forbidden, "Sender is not a participant in this chat session.", null);
                }

                ChatMessage newMessage = new ChatMessage(session, sender, content);

                // Add message
                ChatMessage? createdMessage = await _chatMessageDao.AddAsync(newMessage);

                if (createdMessage == null)
                {
                    _logger.LogError("Failed to add message from sender {SenderId} to session {SessionId} in DB.", senderId, sessionId);
                    return new Response(StatusCodes.Status500InternalServerError, "Failed to send message.", null);
                }

                ChatMessage? detailedMessage = await _chatMessageDao.GetByIdAsync(createdMessage.Id);

                return new Response(StatusCodes.Status201Created, "Message sent successfully.", detailedMessage ?? createdMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from sender {SenderId} to session {SessionId}.", senderId, sessionId);
                return new Response(StatusCodes.Status500InternalServerError, "An error occurred while sending the message.", null);
            }
        }
    }
}