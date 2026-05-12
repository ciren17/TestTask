namespace BackgroundWorker;

using System.Text;
using System.Text.Json;

using Application.Interfaces;
using Application.Interfaces.Messaging;
using Application.Messages;
using Domain.Enums;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Worker : BackgroundService
{
    private const string QueueName = "pdf-processing";

    private readonly IRabbitMqConnectionProvider _provider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    private IChannel? _channel;

    public Worker(
        IRabbitMqConnectionProvider provider,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _provider = provider;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connection =
            await _provider.GetConnectionAsync();

        _channel = await connection.CreateChannelAsync(
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += ProcessMessageAsync;

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation(
            "RabbitMQ consumer started");

        await Task.Delay(
            Timeout.Infinite,
            stoppingToken);
    }

    private async Task ProcessMessageAsync(
        object sender,
        BasicDeliverEventArgs eventArgs)
    {
        try
        {
            var json = Encoding.UTF8.GetString(
                eventArgs.Body.ToArray());

            _logger.LogInformation(
                "Received message: {Message}",
                json);

            var message = JsonSerializer.Deserialize<PdfUploadedMessage>(
                json);

            if (message is null)
            {
                _logger.LogWarning(
                    "Message deserialization failed");

                await RejectMessageAsync(eventArgs);

                return;
            }

            using var scope = _scopeFactory.CreateScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IPdfRepository>();

            var extractor =
                scope.ServiceProvider
                    .GetRequiredService<ITextExtractor>();

            var document = await repository.GetByIdAsync(
                message.DocumentId,
                CancellationToken.None);

            if (document is null)
            {
                _logger.LogWarning(
                    "Document not found: {DocumentId}",
                    message.DocumentId);

                await RejectMessageAsync(eventArgs);

                return;
            }

            try
            {
                document.Status = DocumentStatus.Processing;

                await repository.SaveChangesAsync(
                    CancellationToken.None);

                _logger.LogInformation(
                    "Started processing document {DocumentId}",
                    document.Id);

                var text = await extractor.ExtractAsync(
                    document.FilePath,
                    CancellationToken.None);

                document.ExtractedText = text;
                document.Status = DocumentStatus.Completed;
                document.ProcessedAt = DateTime.UtcNow;

                await repository.SaveChangesAsync(
                    CancellationToken.None);

                _logger.LogInformation(
                    "Document processed successfully: {DocumentId}",
                    document.Id);

                await _channel!.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing document {DocumentId}",
                    document.Id);

                document.Status = DocumentStatus.Failed;

                await repository.SaveChangesAsync(
                    CancellationToken.None);

                await RejectMessageAsync(eventArgs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled consumer error");

            await RejectMessageAsync(eventArgs);
        }
    }

    private async Task RejectMessageAsync(
        BasicDeliverEventArgs eventArgs)
    {
        if (_channel is null)
            return;

        await _channel.BasicNackAsync(
            deliveryTag: eventArgs.DeliveryTag,
            multiple: false,
            requeue: false);
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Stopping RabbitMQ consumer");

        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}