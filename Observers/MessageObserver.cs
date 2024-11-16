using System.Text;
using IO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Observers;

public class MessageObserver : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Password = "guest",
            Port = 5672,
            UserName = "guest",
        };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "g-d-rh",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var contentBytesArray = eventArgs.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentBytesArray);
            var deserializedMessage = System.Text.Json.JsonSerializer.Deserialize<CustomMessage>(
                contentString
            );

            Console.WriteLine(
                $"date: {deserializedMessage.CreatedAt}, super secret stalker: {deserializedMessage.Sender}, super secret stalker message: {deserializedMessage.Message}"
            );

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(queue: "g-d-rh", autoAck: false, consumer);
    }
}
