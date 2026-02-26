using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaEventPublisher> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public KafkaEventPublisher(IConfiguration configuration, ILogger<KafkaEventPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:Topic"] ?? "domain-events";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(T).FullName ?? typeof(T).Name;
        var json = JsonSerializer.Serialize(@event, JsonOptions);

        var message = new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = json,
            Headers = new Headers
            {
                { "event-type", System.Text.Encoding.UTF8.GetBytes(eventType) },
                { "occurred-at", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
            }
        };

        try
        {
            var result = await _producer.ProduceAsync(_topic, message, cancellationToken);
            _logger.LogDebug("Published event {EventType} to {Topic} partition {Partition} offset {Offset}",
                eventType, result.Topic, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to Kafka: {Error}",
                eventType, ex.Error.Reason);
            throw;
        }
    }

    public void Dispose() => _producer.Dispose();
}
