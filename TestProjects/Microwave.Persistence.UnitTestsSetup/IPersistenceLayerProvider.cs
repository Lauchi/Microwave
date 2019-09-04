using Microwave.Discovery;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Ports;
using Microwave.Subscriptions;

namespace Microwave.Persistence.UnitTestsSetup
{
    public abstract class PersistenceLayerProvider
    {
        public abstract IVersionRepository VersionRepository { get; }
        public abstract IRemoteVersionReadRepository RemoteVersionReadRepository { get; }
        public abstract IStatusRepository StatusRepository { get; }
        public abstract IReadModelRepository ReadModelRepository { get; }
        public abstract ISnapShotRepository SnapShotRepository { get; }
        public abstract IEventRepository EventRepository { get; }
        public abstract ISubscriptionRepository SubscriptionRepository { get; }
    }
}