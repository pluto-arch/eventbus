using Event;
using Newtonsoft.Json;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBusRabbitMQ;
using Pluto.EventBusRabbitMQ.Connection;
using RabbitMQ.Client;

namespace Consumer01
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
            eb?.Subscribe<DemoEvent, DemoEventHandler>();

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


            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                return new EventBusRabbitMQ(connection,logger,serializeProvider,new Pluto.EventBusRabbitMQ.Options.RabbitNQDeclaration
                {
                    ExchangeName = "订单广播",
                    QueueName = "票据打印",
                    ExchangeType = ExchangeType.Fanout
                });
            });

            return services;
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