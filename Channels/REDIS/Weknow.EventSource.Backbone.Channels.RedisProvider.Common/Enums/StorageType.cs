using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Channels.RedisProvider;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Bucket storage type
    /// </summary>
    [Flags]
    public enum StorageType 
    {
        None = 0,
        Segments = 1,
        Interceptions = 2,
        All = Segments | Interceptions
    }
}
