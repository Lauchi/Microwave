﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Application.Framework.Results;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EntityStreamRepository : IEntityStreamRepository
    {
        private readonly IObjectConverter _eventConverter;
        private readonly EventStoreWriteContext _eventStoreWriteContext;

        public EntityStreamRepository(IObjectConverter eventConverter, EventStoreWriteContext eventStoreWriteContext)
        {
            _eventConverter = eventConverter;
            _eventStoreWriteContext = eventStoreWriteContext;
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsByEntity(Guid entityId, long from = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString() && str.Version > from).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEvent>>.NotFound(entityId.ToString());

            var domainEvents = stream.Select(dbo =>
                {
                    var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                    domainEvent.Version = dbo.Version;
                    domainEvent.Created = dbo.Created;
                    return domainEvent;
                });

            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result<IEnumerable<DomainEvent>>> LoadEventsSince(long tickSince = -1)
        {
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.Created > tickSince).ToList();
            if (!stream.Any()) return Result<IEnumerable<DomainEvent>>.Ok(new List<DomainEvent>());

            var domainEvents = stream.Select(dbo =>
            {
                var domainEvent = _eventConverter.Deserialize<DomainEvent>(dbo.Payload);
                domainEvent.Version = dbo.Version;
                domainEvent.Created = dbo.Created;
                return domainEvent;
            });

            return Result<IEnumerable<DomainEvent>>.Ok(domainEvents);
        }

        public async Task<Result> AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var events = domainEvents.ToList();
            var entityId = events.First().EntityId;
            var stream = _eventStoreWriteContext.EntityStreams
                .Where(str => str.EntityId == entityId.ToString()).ToList();

            var entityVersionTemp = stream.Count - 1;
            if (entityVersionTemp != entityVersion) return Result.ConcurrencyResult(entityVersionTemp, entityVersion);

            foreach (var domainEvent in events)
            {
                entityVersionTemp = entityVersionTemp + 1;
                var serialize = _eventConverter.Serialize(domainEvent);
                var domainEventDbo = new DomainEventDbo
                {
                    Payload = serialize,
                    Created = DateTimeOffset.UtcNow.Ticks,
                    Version = entityVersionTemp,
                    EntityId = domainEvent.EntityId.ToString()
                };

                _eventStoreWriteContext.EntityStreams.Add(domainEventDbo);
            }

            await _eventStoreWriteContext.SaveChangesAsync();
            return Result.Ok();
        }
    }
}