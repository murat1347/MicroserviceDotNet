using Esourcing.Sourcing.Data;
using Esourcing.Sourcing.Data.Interface;
using Esourcing.Sourcing.Repositories;
using Esourcing.Sourcing.Repositories.Interfaces;
using Esourcing.Sourcing.Settings;
using EventBusRabbitMQ;
using EventBusRabbitMQ.Producer;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SourcingDatabaseSettings>(builder.Configuration.GetSection(nameof(SourcingDatabaseSettings)));
builder.Services.AddSingleton<ISourcingDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<SourcingDatabaseSettings>>().Value);
builder.Services.AddTransient<ISourcingContext, SourcingContext>();
builder.Services.AddTransient<IAuctionRepository, AuctionRepository>();
builder.Services.AddTransient<IBidRepository, BidRepository>();

#region EventBus

builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

    var factory = new ConnectionFactory()
    {
        HostName = builder.Configuration["EventBus:HostName"]
    };

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:UserName"]))
    {
        factory.UserName = builder.Configuration["EventBus:UserName"];
    }

    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:Password"]))
    {
        factory.UserName = builder.Configuration["EventBus:Password"];
    }

    var retryCount = 5;
    if (!string.IsNullOrWhiteSpace(builder.Configuration["EventBus:RetryCount"]))
    {
        retryCount = int.Parse(builder.Configuration["EventBus:RetryCount"]);
    }

    return new DefaultRabbitMQPersistentConnection(factory, retryCount, logger);

    builder.Services.AddSingleton<EventBusRabbitMQProducer>();
});
#endregion
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithOrigins("https://localhost:44398");
}));

//services.AddSignalR()


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
