using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.Persistence.InMemory.Querries
{
    public class VersionRepositoryInMemory : IVersionRepository
    {
        private readonly ConcurrentDictionary<string, DateTimeOffset> _versionDictionray = new ConcurrentDictionary<string, DateTimeOffset>();
        private readonly ConcurrentDictionary<string, DateTimeOffset> _versionDictionrayRemote = new
        ConcurrentDictionary<string, DateTimeOffset>();
        public Task<DateTimeOffset> GetVersionAsync(string domainEventType)
        {
            if (domainEventType == null) return Task.FromResult(DateTimeOffset.MinValue);
            if (!_versionDictionray.TryGetValue(domainEventType, out var version)) return Task.FromResult(DateTimeOffset.MinValue);

            return Task.FromResult(version);
        }

        public Task<DateTimeOffset> GetRemoteVersionAsync(string domainEventType)
        {
            if (domainEventType == null) return Task.FromResult(DateTimeOffset.MinValue);
            if (!_versionDictionrayRemote.TryGetValue(domainEventType, out var version)) return Task.FromResult(DateTimeOffset.MinValue);

            return Task.FromResult(version);
        }

        public Task SaveVersionAsync(LastProcessedVersion version)
        {
            _versionDictionray[version.EventType] = version.LastVersion;
            return Task.CompletedTask;
        }

        public Task SaveRemoteVersionAsync(LastProcessedVersion version)
        {
            _versionDictionrayRemote[version.EventType] = version.LastVersion;
            return Task.CompletedTask;
        }
    }
}