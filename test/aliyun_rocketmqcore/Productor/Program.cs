using Dncy.EventBus.Abstract.EventActivator;
using Dncy.EventBus.AliyunRocketMQCore;
using Dncy.EventBus.AliyunRocketMQCore.Options;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Productor.Event;
using Productor.Services;

namespace Productor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddScoped<IScopedService, ScopedService>();
            builder.Services.AddTransient<ITranService, TranService>();
            builder.Services.AddSingleton<ISignalService, SignalService>();


            builder.Services.AddSingleton<IntegrationEventHandlerActivator>();
            builder.Services.AddSingleton<AliyunRocketEventBusCore>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBusCore>>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="t_preorder",
                    GroupId="GID_preorder",
                    HttpEndPoint="http://1mqrest.uncs.com",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new AliyunRocketEventBusCore(options,msa,null,logger);
            });


            var app = builder.Build();

            var bus = app.Services.GetRequiredService<AliyunRocketEventBusCore>();
            bus.StartBasicConsume();

            app.MapGet("/",
                ([FromServices] IScopedService ss, [FromServices] ISignalService s, [FromServices] ITranService ts) =>
                {
                    var sc = ss.OutPutHashCode();
                    var ssc=s.OutPutHashCode();
                    var sssc=ts.OutPutHashCode();
                    return Results.Json(new
                    {
                        IScopedService=sc,
                        ISignalService=ssc,
                        ITranService=sssc
                    });
                });



            app.MapGet("/pub/userregister/{email}", async ([FromServices]AliyunRocketEventBusCore bus,[FromRoute]string email) =>
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


            app.MapGet("/pub/userdisabled/{email}", async ([FromServices]AliyunRocketEventBusCore bus,[FromRoute]string email) =>
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
        }
    }
}