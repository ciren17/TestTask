namespace Application.Interfaces.Messaging;

using RabbitMQ.Client;

public interface IRabbitMqConnectionProvider
{
    Task<IConnection> GetConnectionAsync();
}