﻿
/* 项目“Dncy.EventBus.AliyunRocketMQCore (net6.0)”的未合并的更改
在此之前:
using System;
using Aliyun.MQ.Model;
在此之后:
using Aliyun.MQ.Model;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQCore (net5.0)”的未合并的更改
在此之前:
using System;
using Aliyun.MQ.Model;
在此之后:
using Aliyun.MQ.Model;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQCore (netcoreapp3.1)”的未合并的更改
在此之前:
using System;
using Aliyun.MQ.Model;
在此之后:
using Aliyun.MQ.Model;
*/
using Microsoft.Extensions.Logging;

/* 项目“Dncy.EventBus.AliyunRocketMQCore (net6.0)”的未合并的更改
在此之前:
using System.Reflection.Emit;
在此之后:
using System;
using System.Reflection.Emit;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQCore (net5.0)”的未合并的更改
在此之前:
using System.Reflection.Emit;
在此之后:
using System;
using System.Reflection.Emit;
*/

/* 项目“Dncy.EventBus.AliyunRocketMQCore (netcoreapp3.1)”的未合并的更改
在此之前:
using System.Reflection.Emit;
在此之后:
using System;
using System.Reflection.Emit;
*/
using System;
#pragma warning disable SYSLIB1006

namespace Dncy.EventBus.AliyunRocketMQCore
{
#if NET6_0_OR_GREATER
    internal static partial class Log
    {
        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Debug,
            Message = "`{message}`")]
        internal static partial void DebugMessage(this ILogger logger, string message);


        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Information,
            Message = "`{message}`")]
        internal static partial void InfoMessage(this ILogger logger, string message);


        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Warning,
            Message = "`{message}`")]
        internal static partial void WarningMessage(this ILogger logger, string message);



        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Warning,
            Message = "event bus `{name}` task has been cancelled.")]
        internal static partial void TaskCancelled(this ILogger logger, string name);



        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Information,
            Message = "successed genetate consumer for topic: `{topic}` . group: `{groupId}`")]
        internal static partial void ConsumerInitialized(this ILogger logger, string topic, string groupId);



        [LoggerMessage(
            EventId = 0,
            Level = LogLevel.Information,
            Message = "consumer message from route: `{route}`. message is : `{message}`")]
        internal static partial void MessageConsumed(this ILogger logger, string route, string message);
    }
#else

    internal static class Log
    {
    #region log defined
        private static readonly Action<ILogger, string, Exception> _generalMessageInfo
            = LoggerMessage.Define<string>(LogLevel.Information, new EventId(0, nameof(InfoMessage)), "{message}");

        private static readonly Action<ILogger, string, Exception> _generalMessageDebug
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(0, nameof(DebugMessage)), "{message}");

        private static readonly Action<ILogger, string, Exception> _generalWarningMessage
            = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(WarningMessage)), "{message}");


        private static readonly Action<ILogger, string, Exception> _taskCancelled
            = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(0, nameof(TaskCancelled)), "event bus {name} task has been cancelled.");


        private static readonly Action<ILogger, string, string, Exception> _consumerInitialized
            = LoggerMessage.Define<string, string>(LogLevel.Information, new EventId(0, nameof(ConsumerInitialized)), "successed genetate consumer for topic: {topic} . group: {groupId}.");


        private static readonly Action<ILogger, string, string, Exception> _messageConsumed
            = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                new EventId(0, nameof(ConsumerInitialized)),
                "message consumed from route: {route} . message: {body}.");

    #endregion


        public static void InfoMessage(this ILogger logger, string message)
        {
            _generalMessageInfo(logger, message, null);
        }


        public static void DebugMessage(this ILogger logger, string message)
        {
            _generalWarningMessage(logger, message, null);
        }

        public static void WarningMessage(this ILogger logger, string message)
        {
            _generalWarningMessage(logger, message, null);
        }


        public static void TaskCancelled(this ILogger logger, string name)
        {
            _taskCancelled(logger, name, null);
        }


        public static void ConsumerInitialized(this ILogger logger, string topic, string groupId)
        {
            _consumerInitialized(logger, topic, groupId, null);
        }


        public static void MessageConsumed(this ILogger logger, string route, string body)
        {
            _messageConsumed(logger, route, body, null);
        }
    }
#endif
}

