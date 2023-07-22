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


            // ע����Ϣ���������Ὣ���е�IntegrationEventHandler��ʹ����Subscribe��ǵķ���ע�������
            builder.Services.AddSingleton<IntegrationEventHandlerActivator>();


            // ע��rabbitmq����
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

            // ע��rabbitmq�¼�����
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
            // ��ʼ����
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