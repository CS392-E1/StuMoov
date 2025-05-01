/**
 * ChatController.cs
 * 
 * Handles chat functionality including sessions and messages between renters and lenders.
 * Provides endpoints for creating and retrieving chat sessions and sending/receiving messages.
 */

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StuMoov.Models;
using StuMoov.Services.ChatService;

namespace StuMoov.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ChatSessionService _chatSessionService;  // Service for managing chat sessions
        private readonly ChatMessageService _chatMessageService;  // Service for managing chat messages
        private readonly ILogger<ChatController> _logger;         // Logging service

        /// <summary>
        /// Initialize the ChatController with required services
        /// </summary>
        public ChatController(
            ChatSessionService chatSessionService,
            ChatMessageService chatMessageService,
            ILogger<ChatController> logger)
        {
            _chatSessionService = chatSessionService;
            _chatMessageService = chatMessageService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all chat sessions for the currently authenticated user
        /// </summary>
        /// <returns>List of chat sessions associated with the user</returns>
        /// <route>GET: api/sessions<route>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetMySessions()
        {
            // Extract user ID from claims in the authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }

            // Retrieve sessions for the authenticated user
            Response response = await _chatSessionService.GetSessionsByUserIdAsync(userId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves a specific chat session by its ID
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session</param>
        /// <returns>Details of the requested chat session</returns>
        /// <route>GET: api/sessions/{sessionId}<route>
        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionById(Guid sessionId)
        {
            // Get the session details by ID
            Response response = await _chatSessionService.GetSessionByIdAsync(sessionId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Finds a chat session by participant IDs and storage location
        /// </summary>
        /// <param name="renterId">ID of the renter participating in the chat</param>
        /// <param name="lenderId">ID of the lender participating in the chat</param>
        /// <param name="storageLocationId">ID of the storage location associated with the chat</param>
        /// <returns>Matching chat session if found</returns>
        /// <route>GET: api/sessions/participants<route>
        [HttpGet("sessions/participants")]
        public async Task<IActionResult> GetSessionByParticipants(
            [FromQuery] Guid renterId,
            [FromQuery] Guid lenderId,
            [FromQuery] Guid storageLocationId)
        {
            // Ensure the current user is a participant in the requested session
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }

            // Security check - current user must be either the renter or lender
            if (currentUserId != renterId && currentUserId != lenderId)
            {
                return Forbid();
            }

            // Retrieve the session matching the participant criteria
            Response response = await _chatSessionService.GetSessionByParticipantsAsync(
                renterId,
                lenderId,
                storageLocationId
            );
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Creates a new chat session between a renter and lender for a specific storage location
        /// </summary>
        /// <param name="request">Session creation details including participant IDs and storage location</param>
        /// <returns>Created chat session details</returns>
        /// <route>POST: api/sessions<route>
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            // Validate request body
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request body.", ModelState));
            }

            // Create the new chat session
            Response response = await _chatSessionService.CreateSessionAsync(
                request.RenterId,
                request.LenderId,
                request.StorageLocationId
            );
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Retrieves all messages for a specific chat session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session</param>
        /// <returns>List of messages in the chat session</returns>
        /// <route>GET: api/sessions/sessionId/messages<route>
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            // Get all messages for the specified session
            Response response = await _chatMessageService.GetMessagesBySessionIdAsync(sessionId);
            return StatusCode(response.Status, response);
        }

        /// <summary>
        /// Sends a new message in a specific chat session
        /// </summary>
        /// <param name="sessionId">The unique identifier of the chat session</param>
        /// <param name="request">Message content to be sent</param>
        /// <returns>Created message details</returns>
        /// <route>POST: api/sessions/sessionId/messages<route>
        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
        {
            // Validate request body
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request body.", ModelState));
            }

            // Extract sender ID from authorization token
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid senderId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }

            // Add the message to the chat session
            Response response = await _chatMessageService.AddMessageAsync(sessionId, senderId, request.Content);
            return StatusCode(response.Status, response);
        }
    }

    /// <summary>
    /// Data model for creating a new chat session
    /// </summary>
    public class CreateSessionRequest
    {
        public Guid RenterId { get; set; }
        public Guid LenderId { get; set; }
        public Guid StorageLocationId { get; set; }
    }

    /// <summary>
    /// Data model for sending a new chat message
    /// </summary>
    public class SendMessageRequest
    {
        public required string Content { get; set; }
    }
}