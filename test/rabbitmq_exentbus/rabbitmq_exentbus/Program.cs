using Dncy.EventBus.Abstract.EventActivator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluto.EventBusRabbitMQ;
using Pluto.EventBusRabbitMQ.Connection;
using Pluto.EventBusRabbitMQ.Options;
using RabbitMQ.Client;
using rabbitmq_exentbus.Event;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IntegrationEventHandlerActivator>();

builder.Services.AddSingleton<IRabbitMQConnection>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQConnection>>();
    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        DispatchConsumersAsync = true
    };
    factory.UserName = "admin";
    factory.Password = "admin";
    return new DefaultRabbitMQConnection(factory, logger);
});

builder.Services.AddSingleton<EventBusRabbitMQ>(sp =>
{
    var connection = sp.GetRequiredService<IRabbitMQConnection>();
    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
    var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
    var declre = new RabbitMQDeclaration
    {
        ExchangeName = "orderExchange1",
        QueueName = "orderCreated2",
        ExchangeType=ExchangeType.Fanout
    };
    return new EventBusRabbitMQ(connection,declre,msa,logger);
});


var app = builder.Build();

var bus = app.Services.GetRequiredService<EventBusRabbitMQ>();
bus.StartBasicConsume();

app.MapGet("/", () => "Hello World!");

app.MapGet("/pub/userregister/{email}", async ([FromServices]EventBusRabbitMQ bus,[FromRoute]string email) =>
{
    if (string.IsNullOrEmpty(email))
    {
        return "input is not an email address";
    }
    await bus.PublishAsync(new UserRegisterEvent
    {
        Email= email
    });
    return "pub successed";
});


app.MapGet("/pub/userdisabled/{email}", async ([FromServices]EventBusRabbitMQ bus,[FromRoute]string email) =>
{
    if (string.IsNullOrEmpty(email))
    {
        return "input is not an email address";
    }
    await bus.PublishAsync(new UserDisabledEvent
    {
        Email= email
    });
    return "pub successed";
});


app.Run();
