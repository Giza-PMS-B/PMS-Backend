using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel.EventDriven;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka;
using Site.Application.FluentValidation;
using Site.Application.Services;
using Site.Infrastrcure.Persistent;

namespace Site.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateSiteDTOValidator>();


        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Register DbContext to resolve to AppDbContext
        builder.Services.AddScoped<IUOW, UOW>();
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
        builder.Services.AddScoped(typeof(IRepo<Model.Entities.Site>), typeof(Repo<Model.Entities.Site>));
        builder.Services.AddScoped<SiteService>();


        // Enable event infrastructure
        builder.Services.AddScoped<IntegrationEventQueue>();
        builder.Services.AddScoped<IIntegrationEventProducer>(provider => provider.GetRequiredService<IntegrationEventQueue>());
        builder.Services.AddScoped<IIntegrationEventQueue>(provider => provider.GetRequiredService<IntegrationEventQueue>());
        builder.Services.AddSingleton<ConcurrentQueue<IntegrationEvent>>();

        builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        builder.Services.AddSingleton<IMessageNameResolver, DefaultMessageNameResolver>();
        builder.Services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();

        builder.Services.AddKafkaBroker(options =>
        {
            options.BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
            options.ClientId = builder.Configuration["Kafka:ClientId"] ?? "SiteService";
            options.Producer = new SharedKernel.MessageBus.Kafka.Configurations.ProducerOptions
            {
                Acks = Confluent.Kafka.Acks.All,
                MessageTimeoutMs = 30000
            };
            options.Consumer = new SharedKernel.MessageBus.Kafka.Configurations.ConsumerOptions
            {
                GroupId = builder.Configuration["Kafka:Consumer:GroupId"] ?? "SiteServiceGroup",
                EnableAutoCommit = false,
                AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest
            };
        });

        // Update UOW registration
        builder.Services.AddScoped<IUOW>(provider => new UOW(
            provider.GetRequiredService<DbContext>(),
            provider.GetRequiredService<IMessagePublisher>(),
            provider.GetRequiredService<IIntegrationEventQueue>()
        ));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}