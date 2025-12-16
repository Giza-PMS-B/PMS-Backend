using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka.Configurations;

namespace SharedKernel.MessageBus.Kafka
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaBroker(
      this IServiceCollection services,
      Action<KafkaOptions> configure)
        {
            services.Configure(configure);

            services.AddSingleton<IMessageNameResolver, DefaultMessageNameResolver>();
            services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();
            services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

            return services;
        }

        public static IServiceCollection AddKafkaConsumer<TEvent, THandler>(
            this IServiceCollection services)
            where TEvent : IntegrationEvent
            where THandler : class, IMessageHandler<TEvent>
        {
            services.AddScoped<IMessageHandler<TEvent>, THandler>();
            services.AddHostedService<KafkaConsumerHostedService<TEvent>>();
            return services;
        }
    }
}
