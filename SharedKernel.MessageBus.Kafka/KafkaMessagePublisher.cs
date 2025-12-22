using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka.Configurations;
using Site.Model.Shared.Events;

namespace SharedKernel.MessageBus.Kafka
{
    public sealed class KafkaMessagePublisher : IMessagePublisher, IDisposable
    {
        private readonly IProducer<string, byte[]> _producer;
        private readonly IMessageNameResolver _resolver;
        private readonly IMessageSerializer _serializer;
        private readonly ILogger<KafkaMessagePublisher> _logger;

        public KafkaMessagePublisher(
            IOptions<KafkaOptions> options,
            IMessageNameResolver resolver,
            IMessageSerializer serializer, ILogger<KafkaMessagePublisher> logger)
        {
            _resolver = resolver;
            _serializer = serializer;
            _logger = logger;

            var config = new ProducerConfig
            {
                BootstrapServers = options.Value.BootstrapServers,
                ClientId = options.Value.ClientId,
                Acks = options.Value.Producer.Acks,
                MessageTimeoutMs = options.Value.Producer.MessageTimeoutMs
            };

            _producer = new ProducerBuilder<string, byte[]>(config).Build();
        }

        public async Task PublishAsync<TEvent>(
            TEvent message, int topicNum = 0,
            CancellationToken cancellationToken = default)
            where TEvent : IntegrationEvent
        {

            var topicType = GetTopicType(topicNum);
            var topic = _resolver.Resolve(topicType);
            _logger.LogInformation("Successfully created Topic Name {topic}", topic);
            var kafkaMessage = new Message<string, byte[]>
            {
                Key = message.EventId.ToString(),
                Value = _serializer.Serialize(message),
                Headers = new Headers
            {
                { MessageHeaders.CorrelationId, System.Text.Encoding.UTF8.GetBytes(message.EventId.ToString()) }
            }
            };
            _logger.LogInformation("Meeage value =  {val}", kafkaMessage.Value);
            await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
        }

        public void Dispose() => _producer.Dispose();

        private Type GetTopicType(int topicNum)
        {
            return topicNum switch
            {
                1 => typeof(SiteCreatedEvent),
                2 => typeof(BookingCreatedEvent),
                _ => throw new ArgumentException($"Unknown topic number: {topicNum}")
            };
        }
    }
}
