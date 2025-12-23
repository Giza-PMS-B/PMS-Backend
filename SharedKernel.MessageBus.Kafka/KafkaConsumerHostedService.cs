using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka.Configurations;

namespace SharedKernel.MessageBus.Kafka
{
    public sealed class KafkaConsumerHostedService<TEvent> : BackgroundService
    where TEvent : IntegrationEvent
    {
        private readonly IServiceProvider _provider;
        private readonly KafkaOptions _options;
        private readonly IMessageNameResolver _resolver;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<KafkaConsumerHostedService<TEvent>> _logger;

        public KafkaConsumerHostedService(
            IServiceProvider provider,
            IOptions<KafkaOptions> options,
            IMessageNameResolver resolver,
            IMessageSerializer serializer,
            ILogger<KafkaConsumerHostedService<TEvent>> logger)
        {
            _provider = provider;
            _options = options.Value;
            _resolver = resolver;
            _serializer = serializer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ensure the BackgroundService never completes - keep it running
            try
            {
                await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka consumer BackgroundService encountered an error");
                // Don't let the service exit - wait and retry
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    try
                    {
                        await Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
                    }
                    catch (Exception retryEx)
                    {
                        _logger.LogError(retryEx, "Kafka consumer retry failed, will retry again in 5 seconds");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }
        }

        private void ConsumeLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.Consumer.GroupId,
                EnableAutoCommit = _options.Consumer.EnableAutoCommit,
                AutoOffsetReset = _options.Consumer.AutoOffsetReset,
                // Add timeout and retry settings
                SessionTimeoutMs = 30000,
                MaxPollIntervalMs = 300000,
                EnablePartitionEof = false
            };

            var topicName = _resolver.Resolve<TEvent>();
            _logger.LogInformation("Starting Kafka consumer for topic {TopicName} with group {GroupId}", topicName, _options.Consumer.GroupId);

            while (!stoppingToken.IsCancellationRequested)
            {
                IConsumer<string, byte[]>? consumer = null;
                try
                {
                    consumer = new ConsumerBuilder<string, byte[]>(config)
                        .SetErrorHandler((_, e) => _logger.LogWarning("Kafka consumer error: {Reason}", e.Reason))
                        .SetLogHandler((_, m) => _logger.LogDebug("Kafka log: {Message}", m.Message))
                        .Build();
                    
                    _logger.LogInformation("Subscribing to topic {TopicName}", topicName);
                    consumer.Subscribe(topicName);
                    _logger.LogInformation("Successfully subscribed to topic {TopicName}", topicName);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var result = consumer.Consume(stoppingToken);
                            
                            // Handle null result (can happen on timeout/error)
                            if (result == null)
                            {
                                _logger.LogDebug("Received null result from Kafka consumer, continuing...");
                                continue;
                            }

                            _logger.LogDebug("Received message from topic {TopicName}, partition {Partition}, offset {Offset}",
                                result.Topic, result.Partition, result.Offset);

                            using var scope = _provider.CreateScope();
                            var handler = scope.ServiceProvider
                                .GetRequiredService<IMessageHandler<TEvent>>();
                            _logger.LogInformation("message before Desrialization {message}", result.Message.Value);
                            var message = _serializer.Deserialize<TEvent>(result.Message.Value);
                            _logger.LogInformation("Desrialized Message {message}", message);

                            _logger.LogInformation("Processing message of type {MessageType}", typeof(TEvent).Name);

                            handler.HandleAsync(message, stoppingToken).GetAwaiter().GetResult();
                            consumer.Commit(result);

                            _logger.LogDebug("Successfully processed and committed message from offset {Offset}", result.Offset);
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation("Kafka consumer for {EventType} is shutting down", typeof(TEvent).Name);
                            break;
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex, "Kafka consume error for event type {EventType}: {Error}", typeof(TEvent).Name, ex.Error.Reason);
                            // Continue loop to retry
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message for event type {EventType}", typeof(TEvent).Name);
                            // Continue loop to retry
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize or maintain Kafka consumer for {EventType}. Will retry in 5 seconds.", typeof(TEvent).Name);
                    consumer?.Dispose();
                    // Wait before retrying
                    try
                    {
                        Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).Wait(stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                finally
                {
                    consumer?.Dispose();
                }
            }
        }
    }
}
