using Microsoft.AspNetCore.Mvc;
using StuMoov.Services.MessageService;   // namespace that holds MessageService
using StuMoov.Models.MessageModel;

[ApiController]
[Route("api/messages")]
public class MessageController : Controller
{
    private readonly MessageService _messageService;

    public MessageController()          // ←  no args
    {
        _messageService = new MessageService();
    }

    [HttpPost]
    public async Task<ActionResult> SendMessage([FromBody] Message message)
    {
        var response = await _messageService.SendMessageAsync(message);
        return StatusCode(response.Status, response);
    }

    [HttpGet]
    public async Task<ActionResult> GetMessages([FromQuery] Guid user1, [FromQuery] Guid user2)
    {
        var response = await _messageService.GetConversationAsync(user1, user2);
        return StatusCode(response.Status, response);
    }
}