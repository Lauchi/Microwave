﻿using Microsoft.Azure.Documents.Client;

namespace Microwave.Persistence.CosmosDb
{
    public interface ICosmosDb
    {
        string DatabaseId { get; }

        string EventsCollectionId { get; }
        string SnapshotsCollectionId { get; }
        string ServiceMapCollectionId { get; }

        string StatusCollectionId { get; }
        string VersionCollectionId { get; }
        DocumentClient GetCosmosDbClient();
    }
}