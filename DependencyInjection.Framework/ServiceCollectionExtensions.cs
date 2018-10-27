﻿using System.Reflection;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Framework.Queries;
using Adapters.Framework.Subscriptions;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection.Framework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyEventStoreDependencies(this IServiceCollection services,
            Assembly assembly, IConfiguration configuration)
        {
            services.AddTransient<IEventStoreFacade, EventStore>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IObjectConverter, ObjectConverter>();
            services.AddDbContext<EventStoreContext>(option => option.UseSqlite("Data Source=Eventstore.db"));
            services.AddDbContext<SubscriptionContext>(option => option.UseSqlite("Data Source=SubscriptionContext.db"));
            services.AddDbContext<QueryStorageContext>(option => option.UseSqlite("Data Source=QueryStorageContext.db"));
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IVersionRepository, VersionRepository>();
            services.AddTransient<IQeryRepository, QueryRepository>();

            services.AddTransient<AsyncEventDelegator>();

            services.AddSingleton(new EventLocationConfig(configuration));

            return services;
        }
    }
}