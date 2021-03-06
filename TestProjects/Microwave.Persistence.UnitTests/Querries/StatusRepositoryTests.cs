using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.Persistence.UnitTestsSetup;

namespace Microwave.Persistence.UnitTests.Querries
{
    [TestClass]
    public class StatusRepositoryTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_HappyPath(PersistenceLayerProvider layerProvider)
        {
            var statusRepository = layerProvider.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2"),
                    new EventSchema("Event3")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            var microwaveServiceNodes = location.Services.ToList();
            Assert.IsNotNull(microwaveServiceNodes);
            Assert.AreEqual(1, microwaveServiceNodes.Count);
            Assert.AreEqual(2, microwaveServiceNodes[0].SubscribedEvents.Count());
            Assert.AreEqual(1, microwaveServiceNodes[0].ReadModels.Count());
            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_NoSaveBeforeGet(PersistenceLayerProvider layerProvider)
        {
            var statusRepository = layerProvider.StatusRepository;

            var location = await statusRepository.GetEventLocation();

            Assert.IsNotNull(location);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_OneUnresolvedEvent(PersistenceLayerProvider layerProvider)
        {
            var statusRepository = layerProvider.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event3")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Event2", location.UnresolvedEventSubscriptions.Single().Name);
            Assert.IsTrue(!location.UnresolvedReadModeSubscriptions.Any());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task StatusLoadAndSafe_OneUnresolvedReadModel(PersistenceLayerProvider layerProvider)
        {
            var statusRepository = layerProvider.StatusRepository;

            List<EventsPublishedByService> services = new List<EventsPublishedByService> {
                EventsPublishedByService.Reachable(new ServiceEndPoint(new Uri("http://service1.de"), "Name1"), new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                })
            };
            var subscribedEventCollection = new EventsSubscribedByService(
                new []
                {
                    new EventSchema("Event1"),
                    new EventSchema("Event2")
                },
                new []{ new ReadModelSubscription("Rm1", new EventSchema("Event3")) });
            var eventLocation = new EventLocation(services, subscribedEventCollection);

            await statusRepository.SaveEventLocation(eventLocation);
            await statusRepository.SaveEventLocation(eventLocation);
            var location = await statusRepository.GetEventLocation();

            Assert.AreEqual("Rm1", location.UnresolvedReadModeSubscriptions.Single().ReadModelName);
            Assert.AreEqual("Event3", location.UnresolvedReadModeSubscriptions.Single().GetsCreatedOn.Name);
            Assert.IsTrue(!location.UnresolvedEventSubscriptions.Any());
        }


        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task SaveAndLoadServiceMap(PersistenceLayerProvider layerProvider)
        {
            var statusRepository = layerProvider.StatusRepository;

            var map = new ServiceMap(new List<MicrowaveServiceNode>
            {
                MicrowaveServiceNode.ReachableMicrowaveServiceNode(new ServiceEndPoint(new Uri("http://123.de"), "Name"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://www.uri1.de"), "Name")
                    }),
                MicrowaveServiceNode.ReachableMicrowaveServiceNode(new ServiceEndPoint(new Uri("http://123.de"), "Name2"),
                    new List<ServiceEndPoint>
                    {
                        new ServiceEndPoint(new Uri("http://www.uri2.de"), "Name2"),
                        new ServiceEndPoint(new Uri("http://www.uri1.de"), "Name")
                    }),
                MicrowaveServiceNode.UnreachableMicrowaveServiceNode(new ServiceEndPoint(new Uri("http://123.de")),
                    new List<ServiceEndPoint>())
            });
            await statusRepository.SaveServiceMap(map);
            var mapLoaded = await statusRepository.GetServiceMap();

            var serviceDependenciesDtos = mapLoaded.AllServices.ToList();
            Assert.AreEqual("Name", serviceDependenciesDtos[0].ServiceEndPoint.Name);
            Assert.AreEqual("Name2", serviceDependenciesDtos[1].ServiceEndPoint.Name);
            Assert.IsTrue(serviceDependenciesDtos[1].IsReachable);
            Assert.IsFalse(serviceDependenciesDtos[2].IsReachable);
            Assert.AreEqual(new Uri("http://www.uri1.de"), serviceDependenciesDtos[0].ConnectedServices.First().ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://www.uri2.de"), serviceDependenciesDtos[1].ConnectedServices.First().ServiceBaseAddress);
            Assert.AreEqual(new Uri("http://www.uri1.de"), serviceDependenciesDtos[1].ConnectedServices.Skip(1).First().ServiceBaseAddress);
            Assert.AreEqual("Name", serviceDependenciesDtos[0].ConnectedServices.First().Name);
            Assert.AreEqual("Name2", serviceDependenciesDtos[1].ConnectedServices.First().Name);
            Assert.AreEqual("Name", serviceDependenciesDtos[1].ConnectedServices.Skip(1).First().Name);
        }
    }
}