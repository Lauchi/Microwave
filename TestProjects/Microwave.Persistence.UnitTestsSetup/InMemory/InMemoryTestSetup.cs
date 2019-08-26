using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Persistence.InMemory.Eventstores;
using Microwave.Persistence.InMemory.Querries;
using Microwave.Queries;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.UnitTestsSetup.InMemory
{
    public class InMemoryTestSetup : PersistenceLayerProvider
    {
        public override IVersionRepository VersionRepository => new VersionRepositoryInMemory();
        public override IRemoteVersionRepository RemoteVersionRepository => new RemoteVersionRepositoryInMemory();
        public override IStatusRepository StatusRepository => new StatusRepositoryInMemory();
        public override IReadModelRepository ReadModelRepository => new ReadModelRepositoryInMemory();
        public override ISnapShotRepository SnapShotRepository => new SnapShotRepositoryInMemory();
        public override IEventRepository EventRepository => new EventRepositoryInMemory();
    }
}