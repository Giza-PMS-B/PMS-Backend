using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.AspNetCore;
using Invoice.Application.FluentValidation;
using Invoice.Application.Services;
using Invoice.Infrastrcure.Persistent;
using Invoice.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.EventDriven;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka;
using SharedKernel.Logging;
using Serilog;
using Invoice.Application.EventHandlers;
using Booking.Model.Shared;
using DotNetEnv;

namespace Invoice.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add environment variables to configuration
        builder.Configuration.AddEnvironmentVariables();

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
        builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceDTOValidator>();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Register DbContext to resolve to AppDbContext
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

        builder.Services.AddScoped<IUOW, UOW>();
        builder.Services.AddScoped(typeof(IRepo<Model.Entities.Invoice>), typeof(Repo<Model.Entities.Invoice>));
        builder.Services.AddScoped(typeof(IRepo<Ticket>), typeof(Repo<Ticket>));
        builder.Services.AddScoped<TicketService>();
        builder.Services.AddScoped<InvoiceService>();

        builder.Services.AddSingleton<ConcurrentQueue<IntegrationEvent>>();
        // very importatant to uncomment that when dealing with events
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
        }).AddKafkaConsumer<BookingCreatedEvent, BookingCreatedEventHandler>();


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

        // Add logging middleware
        app.UseSharedKernelLogging();

        app.MapControllers();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        try
        {
            Log.Information("Starting Invoice API");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Invoice API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}