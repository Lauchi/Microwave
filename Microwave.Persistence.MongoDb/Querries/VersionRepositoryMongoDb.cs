using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microwave.Queries.Ports;
using MongoDB.Driver;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class VersionRepositoryMongoDb : IVersionRepository
    {
        private readonly IMongoDatabase _dataBase;
        private readonly string _lastProcessedVersions = "LastProcessedVersions";

        public VersionRepositoryMongoDb(MicrowaveMongoDb dataBase)
        {
            _dataBase = dataBase.Database;
        }

        public async Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);
            var lastProcessedVersion =
                (await mongoCollection.FindAsync(version => version.EventType == domainEventType)).FirstOrDefault();
            var ret = lastProcessedVersion == null ? DateTimeOffset.MinValue : lastProcessedVersion.LastVersion;
            return ret;
        }

        public async Task SaveVersionAsync(LastProcessedVersion version)
        {
            var mongoCollection = _dataBase.GetCollection<LastProcessedVersionDbo>(_lastProcessedVersions);

            var findOneAndReplaceOptions = new FindOneAndReplaceOptions<LastProcessedVersionDbo>();
            findOneAndReplaceOptions.IsUpsert = true;

            await mongoCollection.FindOneAndReplaceAsync(
                (Expression<Func<LastProcessedVersionDbo, bool>>) (e => e.EventType == version.EventType),
                new LastProcessedVersionDbo
                {
                    EventType = version.EventType,
                    LastVersion = version.LastVersion
                }, findOneAndReplaceOptions);
        }
    }
}