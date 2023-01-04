using Dncy.EventBus.Abstract.Interfaces;
using Event;
using Newtonsoft.Json;
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

            return services;
        }
    }

}