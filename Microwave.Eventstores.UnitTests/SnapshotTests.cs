using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class SnapshotTests
    {
        [TestMethod]
        public async Task SnapshotRealized()
        {
            var runner = MongoDbRunner.Start("SnapshotRealized");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("SnapshotRealized");
            var mongoCollection = database.GetCollection<SnapShotDbo>("SnapShotDbos");

            var repo = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());
            var eventStore = new EventStore(repo, new SnapShotRepository(database , new ObjectConverter()));

            var entityId = Guid.NewGuid();
            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event1(entityId),
                new Event2(entityId, "Peter")
            }, 0);

            await eventStore.LoadAsync<User>(entityId);

            var snapShotDboOld = (await mongoCollection.FindAsync(entityId.ToString())).ToList().FirstOrDefault();

            Assert.IsNull(snapShotDboOld);

            await eventStore.AppendAsync(new List<IDomainEvent>
            {
                new Event3(entityId, 14),
                new Event2(entityId, "PeterNeu")
            }, 2);

            var eventstoreResult = await eventStore.LoadAsync<User>(entityId);

            var user = eventstoreResult.Entity;
            Assert.AreEqual(4, eventstoreResult.Version);
            Assert.AreEqual(14, user.Age);
            Assert.AreEqual("PeterNeu", user.Name);
            Assert.AreEqual(entityId, user.Id);

            var snapShotDbo = (await mongoCollection.FindAsync(entityId.ToString())).ToList().First();

            Assert.AreEqual(4, snapShotDbo.Version);
            Assert.AreEqual(entityId.ToString(), snapShotDbo.EntityId);
            var userSnapShot = new ObjectConverter().Deserialize<User>(snapShotDbo.Payload);

            Assert.AreEqual(14, userSnapShot.Age);
            Assert.AreEqual("PeterNeu", userSnapShot.Name);
            Assert.AreEqual(entityId, userSnapShot.Id);

            client.DropDatabase("SnapshotRealized");
            runner.Dispose();
        }
    }

    [SnapShotAfter(3)]
    public class User : Entity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Guid Id { get; set; }

        public void Apply(Event1 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public void Apply(Event2 domainEvent)
        {
            Name = domainEvent.Name;
        }

        public void Apply(Event3 domainEvent)
        {
            Age = domainEvent.Age;
        }
    }

    public class Event1 : IDomainEvent
    {
        public Event1(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public class Event2 : IDomainEvent
    {
        public Event2(Guid entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Guid EntityId { get; }
        public string Name { get; }
    }

    public class Event3 : IDomainEvent
    {
        public Event3(Guid entityId, int age)
        {
            EntityId = entityId;
            Age = age;
        }

        public Guid EntityId { get; }
        public int Age { get; }
    }
}