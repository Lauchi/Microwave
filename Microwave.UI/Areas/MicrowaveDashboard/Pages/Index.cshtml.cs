﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.WebApi;

namespace Microwave.UI.Areas.MicrowaveDashboard.Pages
{
    public class IndexModel : MicrowavePageModel
    {
        private readonly IDiscoveryHandler _discoveryHandler;
        public EventLocation ConsumingServices { get; set; }
        public EventsPublishedByService PublishedEvents { get; set; }

        public bool HasMissingEvents => ConsumingServices.UnresolvedEventSubscriptions.Any()
                                        || ConsumingServices.UnresolvedReadModeSubscriptions.Any();

        public IndexModel(
            IDiscoveryHandler discoveryHandler,
            MicrowaveWebApiConfiguration configuration) : base(configuration)
        {
            _discoveryHandler = discoveryHandler;
        }

        public async Task OnGetAsync()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServices();
            var publishedEvents = await _discoveryHandler.GetPublishedEvents();
            ConsumingServices = consumingServices;
            PublishedEvents = publishedEvents;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Redirect("/MicrowaveDashboard");
        }
    }
}