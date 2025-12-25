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
using SharedKernel.Logging;
using Site.Application.FluentValidation;
using Site.Application.Services;
using Site.Infrastrcure.Persistent;
using Serilog;

namespace Site.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        // Add Serilog logging
        builder.AddSerilogLogging();

        // Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add shared kernel logging services
        builder.Services.AddSharedKernelLogging();

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

        builder.Services.AddSingleton<ConcurrentQueue<IntegrationEvent>>();

        builder.Services.AddScoped<IIntegrationEventProducer, IntegrationEventQueue>();
        builder.Services.AddScoped<IIntegrationEventQueue, IntegrationEventQueue>();

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


        builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        builder.Services.AddSingleton<IMessageNameResolver, DefaultMessageNameResolver>();
        builder.Services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();


        var app = builder.Build();

        // 🔹 Apply migrations automatically
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseCors("AllowAll");
        // Add logging middleware
        app.UseSharedKernelLogging();

        app.MapControllers();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        try
        {
            Log.Information("Starting Site API");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Site API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}