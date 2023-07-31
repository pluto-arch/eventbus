using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dncy.EventBus.SubscribeActivator
{
    public class IntegrationEventHandlerActivator
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<IntegrationEventHandlerActivator> _logger;

        private static readonly List<string> _disabledSubscribe = new List<string>();
        private readonly ImmutableList<string> disabled = _disabledSubscribe.ToImmutableList();

        private static readonly Lazy<ConcurrentBag<SubscribeDescriptor>> _lazySubscribes = 
            new Lazy<ConcurrentBag<SubscribeDescriptor>>(EnsureSubscribeDescriptorsInitialized, true);
        private readonly ImmutableList<SubscribeDescriptor> subscribes = _lazySubscribes.Value.ToImmutableList();

        private static readonly Lazy<ConcurrentDictionary<Type, ObjectFactory>> _lazyCacheObjFactory 
            = new Lazy<ConcurrentDictionary<Type, ObjectFactory>>(() => new ConcurrentDictionary<Type, ObjectFactory>(), true);


        public IntegrationEventHandlerActivator(
            IServiceScopeFactory scopeFactory,
            ILogger<IntegrationEventHandlerActivator> logger = null)
        {
            _scopeFactory = scopeFactory;
            _logger = logger?? NullLogger<IntegrationEventHandlerActivator>.Instance;;
        }

        public async Task ProcessRequestAsync(string route, string message, IDictionary<string, string> properties, string eventBusName)
        {
            await ProcessRequestAsync( route,  message, properties,  eventBusName);
        }
        
        public async Task ProcessRequestAsync(string route, string message,IDictionary<string,object> properties,string eventBusName)
        {
            using (var sc = _scopeFactory.CreateScope())
            {
                foreach (SubscribeDescriptor subscribeDescriptor in subscribes.Where(x => !disabled.Contains(x.Id)&&x.SubscribeEventBusName==eventBusName).OrderBy(x => x.Order))
                {
                    RouteValueDictionary matchedRouteValues = new RouteValueDictionary();

                    if (RouteMatcher.TryMatch(subscribeDescriptor.AttributeRouteInfo.RouteTemplate, route, matchedRouteValues))
                    {
                        var parameterValues = new List<object>();

                        var valueProviders = new Dictionary<string, object>(matchedRouteValues, StringComparer.OrdinalIgnoreCase) { { string.Empty, message } };

                        foreach (var parameterInfo in subscribeDescriptor.Parameters)
                        {
                            parameterValues.Add(BindModelAsync(parameterInfo, valueProviders));
                        }


                        var instanceType = subscribeDescriptor.MethodInfo.DeclaringType;

                        if (instanceType != null)
                        {
                            var createFactory = CreateOrCacheObjectFactory(instanceType);
                            if (createFactory == null)
                            {
                                throw new InvalidOperationException($"unable create {instanceType.Name} type");
                            }
                            var handler = (IntegrationEventHandler)createFactory(sc.ServiceProvider, arguments: null);
                            handler.Context = new IntegrationEventContext { Body = message,Properties=properties };
                            if (subscribeDescriptor.MethodInfo.ReturnType.IsAssignableTo(typeof(IAsyncResult)))
                            {
                                var task = (Task)subscribeDescriptor.MethodInfo.Invoke(handler, parameterValues.ToArray());
                                task ??= Task.CompletedTask;
                                await task;
                                continue;
                            }
                            subscribeDescriptor.MethodInfo.Invoke(handler, parameterValues.ToArray());
                        }
                    }
                }
            }
        }

        public void DisableEventHandler(string id)
        {
            _disabledSubscribe.Add(id);
        }

        public void EnableEventHandler(string id)
        {
            _disabledSubscribe.Remove(id);
        }


        public List<SubscribeDescriptorItemModel> SubscribeList()
        {
            var res = new List<SubscribeDescriptorItemModel>();
            foreach (var item in subscribes)
            {
                var disabledInfo = disabled.Any(x => x == item.Id);
                res.Add(new SubscribeDescriptorItemModel
                {
                    Id = item.Id,
                    RouteTemplate = item.AttributeRouteInfo.RouteTemplate,
                    Order = item.Order,
                    MethodInfo = $"{item.MethodInfo.DeclaringType.Namespace}{item.MethodInfo.Name}",
                    Parames = item.Parameters != null ? JsonSerializer.Serialize(item.Parameters.Select(x => x.Name)) : string.Empty,
                    SubscribeEventBusName = item.SubscribeEventBusName,
                    Enabled = !disabledInfo
                });
            }
            return res;
        }

        private static ObjectFactory CreateOrCacheObjectFactory(Type instanceType)
        {
            ObjectFactory createFactory;
            if (_lazyCacheObjFactory.Value.ContainsKey(instanceType))
            {
                createFactory = _lazyCacheObjFactory.Value[instanceType];
            }
            else
            {
                createFactory = ActivatorUtilities.CreateFactory(instanceType, Type.EmptyTypes);
                if (createFactory != null)
                {
                    _lazyCacheObjFactory.Value.TryAdd(instanceType, createFactory);
                }
            }

            return createFactory;
        }
        private object BindModelAsync(ParameterInfo parameterInfo, Dictionary<string, object> valueProviders)
        {
            object parameterValue = parameterInfo.ParameterType.GetDefaultValue();

            try
            {
                if (parameterInfo.Name != null && valueProviders.TryGetValue(parameterInfo.Name, out object value))
                {
                    parameterValue = Convert.ChangeType(value, parameterInfo.ParameterType);
                }
                else if (parameterInfo.ParameterType != typeof(object) && Type.GetTypeCode(parameterInfo.ParameterType) == TypeCode.Object)
                {
                    var modelValue = valueProviders[string.Empty]?.ToString();
                    parameterValue ??= modelValue != null ? JsonSerializer.Deserialize(modelValue, parameterInfo.ParameterType) : parameterValue;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Binding parameters {parameterInfo.Name} failed", e);
            }

            return parameterValue;
        }
        private static ConcurrentBag<SubscribeDescriptor> EnsureSubscribeDescriptorsInitialized()
        {
            ConcurrentBag<SubscribeDescriptor> subscribeDescriptors = new ConcurrentBag<SubscribeDescriptor>();

            var exportedTypes = AppDomain.CurrentDomain.GetAssemblies().Where(e => !e.IsDynamic).SelectMany(e => e.ExportedTypes);

            var handlerImplementationTypes = exportedTypes.Where(t => t.IsAssignableTo(typeof(IntegrationEventHandler)) && t.IsClass);

            foreach (var implementationType in handlerImplementationTypes)
            {
                var methodInfos = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var methodInfo in methodInfos)
                {
                    SubscribeAttribute subscribeAttribute = methodInfo.GetCustomAttribute<SubscribeAttribute>();
                    if (subscribeAttribute != null)
                    {
                        SubscribeDescriptor subscribeDescriptor = new SubscribeDescriptor()
                        {
                            AttributeRouteInfo = subscribeAttribute,
                            MethodInfo = methodInfo,
                            Parameters = methodInfo.GetParameters(),
                            SubscribeEventBusName= subscribeAttribute.SubscribeEventBusName
                        };
                        subscribeDescriptors.Add(subscribeDescriptor);
                    }
                }
            }

            return subscribeDescriptors;
        }
    }
}

