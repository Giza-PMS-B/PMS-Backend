
using SharedKernel.EventDriven;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka;
using SharedKernel.MessageBus.Kafka.Configurations;

namespace ServiceTemplate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IUOW, UOW>();
            builder.Services.AddScoped(typeof(IRepo<>), typeof(Repo<>));
            builder.Services.AddScoped<IIntegrationEventProducer, IntegrationEventQueue>();
            builder.Services.AddScoped<IIntegrationEventQueue, IntegrationEventQueue>();

            // builder.Services.AddKafkaBroker(options =>
            // {
            //     options.BootstrapServers = "localhost:9092";
            //     options.ClientId = "ServiceTemplate";
            //     options.Producer = new ProducerOptions
            //     {
            //         Acks = Confluent.Kafka.Acks.All,
            //         MessageTimeoutMs = 30000,

            //     };
            //     options.Consumer = new ConsumerOptions
            //     {
            //         GroupId = "ServiceTemplateGroup",
            //         EnableAutoCommit = false,
            //         AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            //     };
            // })
            //     .AddKafkaConsumer<OrderCreatedEvent, OrderCreatedHandler>()
            //     .AddKafkaConsumer<PaymentEvent, PaymentHandler>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
