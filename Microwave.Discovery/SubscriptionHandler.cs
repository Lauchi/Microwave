using System;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;

namespace Microwave.Discovery
{
    public class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly EventsSubscribedByService _eventsSubscribedByService;
        private readonly IStatusRepository _statusRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionHandler(EventsSubscribedByService eventsSubscribedByService, IStatusRepository statusRepository, ISubscriptionRepository subscriptionRepository)
        {
            _eventsSubscribedByService = eventsSubscribedByService;
            _statusRepository = statusRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task SubscribeOnDiscoveredServices()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            foreach (var subscribedEvent in _eventsSubscribedByService.Events)
            {
                var microwaveServiceNode = eventLocation.GetServiceForEvent(subscribedEvent.Name);
                if (microwaveServiceNode == null) continue;
                await _subscriptionRepository.SubscribeForEvent(
                    new Subscription(subscribedEvent.Name, microwaveServiceNode.ServiceEndPoint.ServiceBaseAddress));
            }
        }

        public Task SubscribeForEvent(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task StoreSubscription(Subscription subscription)
        {
            throw new NotImplementedException();
        }

        public Task PushNewChanges()
        {
            //load all changes
            //publish them
            throw new NotImplementedException();
        }
    }
}