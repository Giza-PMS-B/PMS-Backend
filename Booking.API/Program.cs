using System.Collections.Concurrent;
using Booking.Application.DTO;
using Booking.Application.EventHandlers;
using Booking.Application.FluentValidation;
using Booking.Application.Services;
using Booking.Infrastrcure.Persistent;
using Booking.Model.Entities;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SharedKernel.EventDriven;
using SharedKernel.EventDriven.Abstraction;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
using SharedKernel.MessageBus.Abstraction;
using SharedKernel.MessageBus.Kafka;
using SharedKernel.Logging;
using Site.Model.Shared;
using Serilog;

namespace Booking.API;

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

        builder.Services.AddAuthorization();
        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Add shared kernel logging services
        builder.Services.AddSharedKernelLogging();

        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<CreateTicketDTOValidator>();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

        builder.Services.AddScoped<IUOW, UOW>();
        builder.Services.AddScoped(typeof(IRepo<Ticket>), typeof(Repo<Ticket>));
        builder.Services.AddScoped(typeof(IRepo<Model.Entities.Site>), typeof(Repo<Model.Entities.Site>));
        builder.Services.AddScoped<TicketService>();
        builder.Services.AddScoped<SiteService>();


        builder.Services.AddSingleton<ConcurrentQueue<IntegrationEvent>>();

        builder.Services.AddScoped<IIntegrationEventProducer, IntegrationEventQueue>();
        builder.Services.AddScoped<IIntegrationEventQueue, IntegrationEventQueue>();

        builder.Services.AddKafkaBroker(options =>
        {
            builder.Configuration.GetSection("Kafka").Bind(options);
        }).AddKafkaConsumer<SiteCreatedEvent, SiteCreatedEventHandler>();

        builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        builder.Services.AddSingleton<IMessageNameResolver, DefaultMessageNameResolver>();
        builder.Services.AddSingleton<IMessageSerializer, JsonMessageSerializer>();


        var app = builder.Build();

        // Apply migrations automatically
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

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
            Log.Information("Starting Booking API");

            // Log when shutdown is requested
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() => Log.Information("Application stopping signal received"));
            lifetime.ApplicationStopped.Register(() => Log.Information("Application stopped"));

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Booking API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
