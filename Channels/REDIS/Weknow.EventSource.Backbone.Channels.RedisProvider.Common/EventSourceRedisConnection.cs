using System.Threading.Tasks;

using StackExchange.Redis;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Event Source connection (for IoC)
    /// Because IConnectionMultiplexer may be used by other component, 
    /// It's more clear to wrap the IConnectionMultiplexer for easier resove by IoC.
    /// </summary>
    /// <param name="ConnectionTask"></param>
    public record EventSourceRedisConnection(Task<IConnectionMultiplexer> ConnectionTask);
}
