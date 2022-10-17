using System.Collections.Generic;
using System.Threading.Tasks;
using Pluto.EventBus.Abstract.Interfaces;

namespace Pluto.EventBus.Abstract
{
    public class NullIntegrationEventStore:IIntegrationEventStore
    {
        /// <inheritdoc />
        public async Task SaveAsync(string eventName, string eventBody,string eventBusName=null)
        {
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetListAllAsync(string eventBusName=null)
        {
            await Task.CompletedTask;
            return default;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetListAsync(string eventTypeName, string eventBusName = null)
        {
            await Task.CompletedTask;
            return default;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetListAsync<T>(string eventTypeName, string eventBusName = null) where T : IntegrationEvent
        {
            await Task.CompletedTask;
            return default;
        }

        public static IIntegrationEventStore Instance { get; set; }=new NullIntegrationEventStore();
    }
}