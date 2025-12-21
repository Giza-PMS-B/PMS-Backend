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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

        private void ConsumeLoop(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                GroupId = _options.Consumer.GroupId,
                EnableAutoCommit = _options.Consumer.EnableAutoCommit,
                AutoOffsetReset = _options.Consumer.AutoOffsetReset
            };

            var topicName = _resolver.Resolve<TEvent>();
            _logger.LogInformation("Starting Kafka consumer for topic {TopicName} with group {GroupId}", topicName, _options.Consumer.GroupId);

            using var consumer = new ConsumerBuilder<string, byte[]>(config).Build();
            consumer.Subscribe(topicName);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    _logger.LogDebug("Received message from topic {TopicName}, partition {Partition}, offset {Offset}", 
                        result.Topic, result.Partition, result.Offset);

                    using var scope = _provider.CreateScope();
                    var handler = scope.ServiceProvider
                        .GetRequiredService<IMessageHandler<TEvent>>();

                    var message = _serializer.Deserialize<TEvent>(result.Message.Value);
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message for event type {EventType}", typeof(TEvent).Name);
                }
            }
        }
    }
}
