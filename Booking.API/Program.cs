using Booking.Application.Services;
using Booking.Infrastrcure.Persistent;
using Booking.Model.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;

namespace Booking.API;

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

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Register DbContext to resolve to AppDbContext
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());

        builder.Services.AddScoped<IUOW, UOW>();
        builder.Services.AddScoped(typeof(IRepo<Ticket>), typeof(Repo<Ticket>));

        // very importatant to uncomment that when dealing with events
        // builder.Services.AddScoped<IIntegrationEventProducer, IntegrationEventQueue>();
        // builder.Services.AddScoped<IIntegrationEventQueue, IntegrationEventQueue>();

        builder.Services.AddScoped<TicketService, TicketService>();


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