namespace Infrastructure.Messaging;

using System.Text;
using System.Text.Json;

using Application.Interfaces;
using Application.Interfaces.Messaging;

using RabbitMQ.Client;

public class RabbitMqPublisher : IMessagePublisher
{
    private readonly IRabbitMqConnectionProvider _provider;

    public RabbitMqPublisher(
        IRabbitMqConnectionProvider provider)
    {
        _provider = provider;
    }

    public async Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken)
    {
        var connection =
            await _provider.GetConnectionAsync();

        await using var channel =
            await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "pdf-processing",
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "pdf-processing",
            body: body,
            cancellationToken: cancellationToken);
    }
}