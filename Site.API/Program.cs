using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.Persistent;
using SharedKernel.Infrastructure.Persistent.Abstraction;
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
        builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<AppDbContext>());
        builder.Services.AddScoped<IUOW, UOW>();
        builder.Services.AddScoped(typeof(IRepo<Model.Entities.Site>), typeof(Repo<Model.Entities.Site>));
        builder.Services.AddScoped<SiteService>();


        // very importatant to uncomment that when dealing with events
        // builder.Services.AddScoped<IIntegrationEventProducer, IntegrationEventQueue>();
        // builder.Services.AddScoped<IIntegrationEventQueue, IntegrationEventQueue>();



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

        app.MapControllers();

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.Run();
    }
}