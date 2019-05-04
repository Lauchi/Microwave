using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Queries;

namespace Microwave.WebApi.Querries
{
    public class EventFeed<T> : IEventFeed<T>
    {
        private readonly DomainEventWrapperListDeserializer _objectConverter;
        private readonly IDomainEventClientFactory _clientFactory;

        public EventFeed(
            DomainEventWrapperListDeserializer objectConverter,
            IDomainEventClientFactory clientFactory)
        {
            _objectConverter = objectConverter;
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
        {
            if (since == default(DateTimeOffset)) since = DateTimeOffset.MinValue;
            var isoString = since.ToString("o");
            var client = await _clientFactory.GetClient<T>();
            try
            {
                if (client.HasTheValidLocation) {
                    var response = await client.GetAsync($"?timeStamp={isoString}");
                    if (response.StatusCode != HttpStatusCode.OK) return new List<DomainEventWrapper>();
                    var content = await response.Content.ReadAsStringAsync();
                    var eventsByTypeAsync = _objectConverter.Deserialize(content);
                    return eventsByTypeAsync;
                }
            }
            catch (HttpRequestException)
            {
                var type = typeof(T);
                var readModel = type.GenericTypeArguments.Single();
                Console.WriteLine($"Could not reach service for: {readModel.Name}");
            }

            return new List<DomainEventWrapper>();
        }
    }
}