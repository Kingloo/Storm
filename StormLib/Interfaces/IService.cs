﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StormLib.Interfaces
{
    public interface IService : IDisposable
    {
        Type HandlesStreamType { get; }

        Task<Result> UpdateAsync(IStream stream, bool preserveSynchronizationContext);
        Task<Result> UpdateAsync(IEnumerable<IStream> streams, bool preserveSynchronizationContext);
    }
}
