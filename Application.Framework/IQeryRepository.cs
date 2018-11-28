﻿using System;
using System.Threading.Tasks;
using Microwave.Application.Results;

namespace Microwave.Application
{
    public interface IQeryRepository
    {
        Task<Result<T>> Load<T>() where T : Query;
        Task<Result<T>> Load<T>(Guid id) where T : IdentifiableQuery;
        Task<Result> Save(Query query);
        Task<Result> Save(IdentifiableQuery query);
    }
}