using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dncy.EventBus.SubscribeActivator
{
    public class SubscribeDescriptor
    {
        public string Id { get; }

        public int Order => AttributeRouteInfo.Order;

        public SubscribeDescriptor()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public SubscribeAttribute AttributeRouteInfo { get; set; } = default!;

        public MethodInfo MethodInfo { get; set; } = default!;

        public IList<ParameterInfo> Parameters { get; set; } = Array.Empty<ParameterInfo>();
    }


    public class SubscribeDescriptorItemModel
    {
        public string Id { get; set; }

        public string RouteTemplate { get; set; }

        public int Order { get; set; }

        public string MethodInfo { get; set; }

        public string Parames { get; set; }
    }
}