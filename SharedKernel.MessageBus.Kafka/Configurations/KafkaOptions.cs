using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace SharedKernel.MessageBus.Kafka.Configurations
{
    public sealed class KafkaOptions
    {
        public string BootstrapServers { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public ProducerOptions Producer { get; set; } = new();
        public ConsumerOptions Consumer { get; set; } = new();
    }

    public sealed class ProducerOptions
    {
        public Acks Acks { get; set; } = Acks.All;
        public int MessageTimeoutMs { get; set; } = 30000;
    }

    public sealed class ConsumerOptions
    {
        public string GroupId { get; set; } = default!;
        public bool EnableAutoCommit { get; set; } = false;
        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    }
}
