using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

namespace Weknow.EventSource.Backbone.ConsoleTests
{
    public static class Constants
    {
        public readonly static string PARTITION = $"test-{DateTime.UtcNow:HH}";
        public readonly static string SHARD_A = $"SHARD_A";
    }
}
