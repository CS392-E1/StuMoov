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
        private readonly ChatSessionService _chatSessionService;
        private readonly ChatMessageService _chatMessageService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            ChatSessionService chatSessionService,
            ChatMessageService chatMessageService,
            ILogger<ChatController> logger)
        {
            _chatSessionService = chatSessionService;
            _chatMessageService = chatMessageService;
            _logger = logger;
        }

        // GET: api/chat/sessions
        // Gets all chat sessions for the currently authenticated user
        [HttpGet("sessions")]
        public async Task<IActionResult> GetMySessions()
        {
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }

            Response response = await _chatSessionService.GetSessionsByUserIdAsync(userId);
            return StatusCode(response.Status, response);
        }

        // GET: api/chat/sessions/{sessionId}
        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessionById(Guid sessionId)
        {
            Response response = await _chatSessionService.GetSessionByIdAsync(sessionId);
            return StatusCode(response.Status, response);
        }

        // GET: api/chat/sessions/participants?renterId={renterId}&lenderId={lenderId}&storageLocationId={storageLocationId}
        [HttpGet("sessions/participants")]
        public async Task<IActionResult> GetSessionByParticipants(
            [FromQuery] Guid renterId,
            [FromQuery] Guid lenderId,
            [FromQuery] Guid storageLocationId)
        {
            // Ensure the current user is in the session
            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }
            if (currentUserId != renterId && currentUserId != lenderId)
            {
                return Forbid();
            }

            Response response = await _chatSessionService.GetSessionByParticipantsAsync(
                renterId,
                lenderId,
                storageLocationId
            );
            return StatusCode(response.Status, response);
        }

        // POST: api/chat/sessions
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request body.", ModelState));
            }

            Response response = await _chatSessionService.CreateSessionAsync(
                request.RenterId,
                request.LenderId,
                request.StorageLocationId
            );
            return StatusCode(response.Status, response);
        }

        // GET: api/chat/sessions/{sessionId}/messages
        [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(Guid sessionId)
        {
            Response response = await _chatMessageService.GetMessagesBySessionIdAsync(sessionId);
            return StatusCode(response.Status, response);
        }

        // POST: api/chat/sessions/{sessionId}/messages
        [HttpPost("sessions/{sessionId}/messages")]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new Response(StatusCodes.Status400BadRequest, "Invalid request body.", ModelState));
            }

            string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid senderId))
            {
                return Unauthorized(new Response(StatusCodes.Status401Unauthorized, "Invalid or missing user ID in token.", null));
            }

            Response response = await _chatMessageService.AddMessageAsync(sessionId, senderId, request.Content);
            return StatusCode(response.Status, response);
        }

    }

    public class CreateSessionRequest
    {
        public Guid RenterId { get; set; }
        public Guid LenderId { get; set; }
        public Guid StorageLocationId { get; set; }
    }

    public class SendMessageRequest
    {
        public required string Content { get; set; }
    }
}