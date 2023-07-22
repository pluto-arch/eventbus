using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.RabbitMQ.Connection;
using Dncy.EventBus.RabbitMQ.Options;
using Dncy.EventBus.SubscribeActivator;
using RabbitMQ.Client;

namespace rabbitmq_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            // 注入消息激活器，会将所有的IntegrationEventHandler中使用了Subscribe标记的方法注册进总线
            builder.Services.AddSingleton<IntegrationEventHandlerActivator>();


            // 注入rabbitmq链接
            builder.Services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    DispatchConsumersAsync = true
                };
                factory.UserName = "admin";
                factory.Password = "admin";
                return new DefaultRabbitMQConnection(factory);
            });

            // 注入rabbitmq事件总线
            builder.Services.AddSingleton<EventBusRabbitMQ>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
                var declre = new RabbitMQDeclaration
                {
                    ExchangeName = "orderExchange1",
                    QueueName = "orderCreated2",
                    ConfigExchangeType = ExchangeType.Fanout
                };
                return new EventBusRabbitMQ(connection, declre, msa);
            });



            var app = builder.Build();


            var bus = app.Services.GetRequiredService<EventBusRabbitMQ>();
            // 开始消费
            bus.StartBasicConsume();

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
        }
    }
}