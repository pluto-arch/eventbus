using Dncy.EventBus.AliyunRocketMQCore;
using Dncy.EventBus.AliyunRocketMQCore.Options;
using Dncy.MQMessageActivator;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pluto.EventBus.Abstract;
using Productor.Event;

namespace Productor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<MessageHandlerActivator>();

            builder.Services.AddSingleton<IMessageSerializeProvider, NewtonsoftMessageSerializeProvider>();

            builder.Services.AddSingleton<AliyunRocketEventBusCore>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBusCore>>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var msa = sp.GetRequiredService<MessageHandlerActivator>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="MQ_INST_1226776583375087_BYV33IZv",
                    Topic="t_user",
                    GroupId="GID_user",
                    HttpEndPoint="http://1226776583375087.mqrest.cn-hangzhou.aliyuncs.com",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "LTAI5tDjbpXS5NupUsnwX7D3",
                        AccessKey = "v11ySL868r6oOTW3rVvlmATiAvtJHO",
                    }
                };
                return new AliyunRocketEventBusCore(options,msa,serializeProvider,null,logger);
            });


            var app = builder.Build();

            _ = app.Services.GetRequiredService<AliyunRocketEventBusCore>();

            app.MapGet("/", ()=>"hello");



            app.MapGet("/pub/{email}", async ([FromServices]AliyunRocketEventBusCore bus,[FromRoute]string email) =>
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


            app.Run();
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
        public T? Deserialize<T>(string objStr)
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