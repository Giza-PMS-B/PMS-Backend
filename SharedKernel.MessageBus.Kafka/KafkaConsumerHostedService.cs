using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

        public KafkaConsumerHostedService(
            IServiceProvider provider,
            IOptions<KafkaOptions> options,
            IMessageNameResolver resolver,
            IMessageSerializer serializer)
        {
            _provider = provider;
            _options = options.Value;
            _resolver = resolver;
            _serializer = serializer;
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

            using var consumer = new ConsumerBuilder<string, byte[]>(config).Build();
            consumer.Subscribe(_resolver.Resolve<TEvent>());

            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);

                using var scope = _provider.CreateScope();
                var handler = scope.ServiceProvider
                    .GetRequiredService<IMessageHandler<TEvent>>();

                var message = _serializer.Deserialize<TEvent>(result.Message.Value);

                handler.HandleAsync(message, stoppingToken).GetAwaiter().GetResult();
                consumer.Commit(result);
            }
        }
    }
}
