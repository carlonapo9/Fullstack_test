using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services.AiAgent;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AiAgentService _agent;

        public ChatController(AiAgentService agent)
        {
            _agent = agent;
        }
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Message))
                return BadRequest("Message cannot be empty.");

            if (string.IsNullOrWhiteSpace(input.SessionId))
                return BadRequest("SessionId is required.");

            var reply = await _agent.ProcessAsync(input.SessionId, input.Message);

            return Ok(new { reply });
        }
    }

    public class ChatInput
    {
        public string SessionId { get; set; }
        public string Message { get; set; }
    }
}
