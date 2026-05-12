namespace Infrastructure.Messaging;

using Application.Interfaces.Messaging;

using Microsoft.Extensions.Configuration;

using RabbitMQ.Client;

public class RabbitMqConnectionProvider
    : IRabbitMqConnectionProvider
{
    private readonly ConnectionFactory _factory;

    private IConnection? _connection;

    public RabbitMqConnectionProvider(
        IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"],
            UserName = configuration["RabbitMQ:User"],
            Password = configuration["RabbitMQ:Password"],

            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is not null &&
            _connection.IsOpen)
        {
            return _connection;
        }

        _connection = await _factory.CreateConnectionAsync();

        return _connection;
    }
}