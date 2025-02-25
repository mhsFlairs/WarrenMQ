using Microsoft.AspNetCore.Mvc;
using WarrenMQ.Contracts;

namespace WarrenMQ.Api;

[ApiController]
[Route("[controller]")]
public class HomeController : ControllerBase
{
    private readonly IRabbitMQService _rabbitMqService;
    private readonly string _exchangeName = "playground-exchange";

    public HomeController(IRabbitMQService rabbitMQService)
    {
        _rabbitMqService = rabbitMQService;
    }

    [HttpPost]
    public async Task<IActionResult> Publish(PublishMessageRequest request, CancellationToken cancellationToken)
    {
        await _rabbitMqService.PublishFanOutMessageAsync(request.Message, _exchangeName, cancellationToken);
        return Ok();
    }
}

public record PublishMessageRequest(string Message);