﻿using System.Collections.Generic;
using Microwave.EventStores.SnapShots;
using Microwave.Queries.Polling;

namespace Microwave.WebApi
{
    public class MicrowaveWebApiConfiguration
    {
        public string ServiceName { get; private set; }
        public ServiceBaseAddressCollection ServiceLocations { get; } = new ServiceBaseAddressCollection();
        public IMicrowaveHttpClientFactory MicrowaveHttpClientFactory { get; private set; } = new DefaultMicrowaveHttpClientFactory();

        public void WithHttpClientFactory(IMicrowaveHttpClientFactory clientFactory)
        {
            MicrowaveHttpClientFactory = clientFactory;
        }

        public IList<ISnapShot> SnapShots { get; } = new List<ISnapShot>();
        public IList<IPollingInterval> PollingIntervals { get; } = new List<IPollingInterval>();

        public void WithServiceName(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}