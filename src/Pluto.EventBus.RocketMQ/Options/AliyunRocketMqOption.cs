/* 项目“Dncy.EventBus.AliyunRocketMQ (netcoreapp3.1)”的未合并的更改
在此之前:
using Microsoft.Extensions.Configuration;
在此之后:
using Microsoft.Extensions.Configuration;
using Pluto;
using Pluto.EventBus;
using Pluto.EventBus.AliyunRocketMQ;
using Dncy.EventBus.AliyunRocketMQ.Options;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQ (netcoreapp3.1)”的未合并的更改
在此之前:
using Microsoft.Extensions.Configuration;
在此之后:
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQ (net5.0)”的未合并的更改
在此之前:
using Microsoft.Extensions.Configuration;
在此之后:
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
*/
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace Dncy.EventBus.AliyunRocketMQ.Options
{

    public static class AliyunRocketMqOptionExtension
    {
        public static void ReadFromConfiguration(this AliyunRocketMqOption options, IConfiguration configuration)
        {
            options = configuration.GetSection("AliyunRocketMQ").Get<AliyunRocketMqOption>();
        }
    }


    /// <summary>
    /// 阿里云rocketmq选项设置
    /// </summary>
    public class AliyunRocketMqOption
    {
        public AliyunRocketMqOption()
        {
            BitchSize = 1;
            WaitSecond = 1;
        }

        /// <summary>Http 端点</summary>
        [Description("Http 端点")]
        public string HttpEndPoint { get; set; }

        /// <summary>主题</summary>
        [Description("主题")]
        public string Topic { get; set; }

        /// <summary>实例id</summary>
        [Description("实例id")]
        public string InstranceId { get; set; }

        /// <summary>消费组</summary>
        [Description("消费组Id")]
        public string GroupId { get; set; }

        /// <summary>单次消费的数量(最低1)</summary>
        [Description("单次消费的数量(最低1)")]
        public uint BitchSize { get; set; }

        /// <summary>长连接时间(1-30)s</summary>
        [Description("长连接时间(1-30)s")]
        public uint WaitSecond { get; set; }

        /// <summary>连接认证配置</summary>
        [Description("连接认证配置")]
        public AuthenticationConfig AuthenticationConfiguration { get; set; }


        public class AuthenticationConfig
        {
            public string AccessId { get; set; }
            public string AccessKey { get; set; }
            public string StsToken { get; set; }
        }
    }
}