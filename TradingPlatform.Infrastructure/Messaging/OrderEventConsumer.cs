using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingPlatform.Domain.Events;

namespace TradingPlatform.Infrastructure.Messaging;

public class OrderEventConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic;
    private readonly ILogger<OrderEventConsumer> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _bootstrapServers;

    public OrderEventConsumer(IConfiguration configuration, ILogger<OrderEventConsumer> logger)
    {
        _logger = logger;

        _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        _topic = configuration["Kafka:OrderEventsTopic"] ?? "order-events";

        var config = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "trading-platform-order-consumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureTopicExists();
        _consumer.Subscribe(_topic);
        _logger.LogInformation("OrderEventConsumer started, subscribed to {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (result is null)
                        continue;

                    ProcessMessage(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from {Topic}", _topic);
                }
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    private void ProcessMessage(ConsumeResult<string, string> result)
    {
        var eventTypeHeader = result.Message.Headers?.FirstOrDefault(h => h.Key == "event-type");
        var eventType = eventTypeHeader is not null
            ? Encoding.UTF8.GetString(eventTypeHeader.GetValueBytes())
            : null;

        if (string.IsNullOrEmpty(eventType))
        {
            _logger.LogWarning("Received message without event-type header, skipping");
            return;
        }

        var json = result.Message.Value;
        if (string.IsNullOrEmpty(json))
        {
            _logger.LogWarning("Received empty message for event type {EventType}", eventType);
            return;
        }

        try
        {
            var domainEvent = DeserializeEvent(eventType, json);
            _logger.LogInformation("Received event: {EventType} - {Event}", eventType, domainEvent?.ToString() ?? json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize event {EventType}: {Json}", eventType, json);
        }
    }

    private void EnsureTopicExists()
    {
        try
        {
            using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build();
            var metadata = admin.GetMetadata(TimeSpan.FromSeconds(10));
            if (metadata.Topics.Any(t => t.Topic == _topic))
                return;

            admin.CreateTopicsAsync(new[] { new TopicSpecification { Name = _topic, NumPartitions = 1, ReplicationFactor = 1 } }).Wait();
            _logger.LogInformation("Created topic {Topic}", _topic);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not ensure topic {Topic} exists (may be created on first publish)", _topic);
        }
    }

    private object? DeserializeEvent(string eventType, string json)
    {
        return eventType switch
        {
            "TradingPlatform.Domain.Events.OrderPlacedEvent" => JsonSerializer.Deserialize<OrderPlacedEvent>(json, JsonOptions),
            "TradingPlatform.Domain.Events.OrderCancelledEvent" => JsonSerializer.Deserialize<OrderCancelledEvent>(json, JsonOptions),
            "TradingPlatform.Domain.Events.OrderExecutedEvent" => JsonSerializer.Deserialize<OrderExecutedEvent>(json, JsonOptions),
            _ => null
        };
    }

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
