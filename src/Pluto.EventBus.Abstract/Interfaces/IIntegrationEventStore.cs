using Dncy.EventBus.Abstract.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dncy.EventBus.Abstract.Interfaces
{
    public interface IIntegrationEventStore
    {
        Task SaveAsync(string eventName,string eventBody, string eventBusName=null);
        
        Task<IEnumerable<string>> GetListAllAsync(string eventBusName = null);
        
        Task<IEnumerable<string>> GetListAsync(string eventTypeName, string eventBusName = null);
        
        Task<IEnumerable<T>> GetListAsync<T>(string eventTypeName, string eventBusName = null) where T : IntegrationEvent;
        
    }
}