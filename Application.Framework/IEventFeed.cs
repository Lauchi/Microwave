using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventFeed<T> where T : IDomainEvent
    {
        Task<IEnumerable<T>> GetEventsByTypeAsync(long lastVersion);
    }
}