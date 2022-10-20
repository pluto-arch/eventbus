using Event;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.RabbitMQ;
using Pluto.EventBus.RabbitMQ.Connection;
using Pluto.EventBus.RabbitMQ.Options;
using RabbitMQ.Client;

namespace Consumer02
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddTransient<UserEventHandler>();
            builder.Services.AddTransient<DemoEventHandler>();
            builder.Services.AddSingleton<IMessageSerializeProvider, NewtonsoftMessageSerializeProvider>();
            builder.Services.AddRabbitMq();
            var app = builder.Build();

            var eb = app.Services.GetService<IEventBus>();
            eb?.Subscribe<UserEvent, UserEventHandler>();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }


    public static class SEX
    {
        // *************rabbitmq
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    DispatchConsumersAsync = true
                };
                factory.UserName = "admin";
                factory.Password = "admin";
                return new DefaultRabbitMQConnection(factory, logger);
            });


            services.AddSingleton<IEventBus, DemoEventBus>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var logger = sp.GetRequiredService<ILogger<DemoEventBus>>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                return new DemoEventBus(connection,logger,serializeProvider,new Pluto.EventBus.RabbitMQ.Options.RabbitNQDeclaration
                {
                    ExchangeName = "订单广播",
                    QueueName = "订单语音播报",
                    ExchangeType = ExchangeType.Fanout
                });
            });

            return services;
        }
    }


    public class DemoEventBus : EventBusRabbitMQ
    {
        /// <inheritdoc />
        public DemoEventBus(IRabbitMQConnection connection, ILogger<EventBusRabbitMQ> logger, IMessageSerializeProvider messageSerializeProvider, RabbitNQDeclaration queueDeclare, IIntegrationEventStore eventStore = null, IEventBusSubscriptionsManager subsManager = null) : base(connection, logger, messageSerializeProvider, queueDeclare, eventStore, subsManager)
        {
        }
    }


    public class NewtonsoftMessageSerializeProvider : IMessageSerializeProvider
    {
        /// <inheritdoc />
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <inheritdoc />
        public T Deserialize<T>(string objStr)
        {
            return JsonConvert.DeserializeObject<T>(objStr);
        }

        /// <inheritdoc />
        public object Deserialize(string objStr, Type type)
        {
            return JsonConvert.DeserializeObject(objStr, type);
        }
    }
}