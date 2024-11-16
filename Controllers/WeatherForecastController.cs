using System.Text;
using IO;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace rabbit_mq_fazueli.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    [HttpPost("send-message")]
    public async Task<ActionResult> SendMessage([FromBody] CustomMessage payload)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Password = "guest",
            Port = 5672,
            UserName = "guest",
        };

        var connection = await factory.CreateConnectionAsync();

        // using creates a local scope and manage an object that will
        // be disposed after the use.
        using (var channel = await connection.CreateChannelAsync())
        {
            var serialized = System.Text.Json.JsonSerializer.Serialize(payload);
            var serializedToBytes = Encoding.UTF8.GetBytes(serialized);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "g-d-rh",
                body: serializedToBytes
            );
        }

        return Ok();
    }
}
