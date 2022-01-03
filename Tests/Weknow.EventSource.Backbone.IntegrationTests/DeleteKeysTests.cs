using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Weknow.EventSource.Backbone.Channels.RedisProvider;
using Weknow.EventSource.Backbone.UnitTests.Entities;
using static Weknow.EventSource.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

using Xunit;
using Xunit.Abstractions;

namespace Weknow.EventSource.Backbone.Tests
{
    public class DeleteKeysTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public DeleteKeysTests(
            ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

        }
        #region DELETE_KEYS_TEST


        [Theory(Skip = "cleanup")]
        [InlineData("*test*")]
        [InlineData("dev:*")]
        [Trait("type", "delete-keys")]
        public async Task DELETE_KEYS_TEST(string pattern)
        {
            IConnectionMultiplexer conn = RedisClientFactory.CreateProviderBlocking(
                                                cfg => cfg.AllowAdmin = true);
            string serverName = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
            var server = conn.GetServer(serverName);
            IEnumerable<RedisKey> keys = server.Keys(pattern: "*test*").ToArray();
            // IEnumerable<RedisKey> keys = server.Keys(pattern: "dev:*").ToArray();
            IDatabaseAsync db = conn.GetDatabase();

            var ab = new ActionBlock<string>(k => LocalAsync(k), new ExecutionDataflowBlockOptions {  MaxDegreeOfParallelism= 30});
            foreach (string key in keys)
            {
                ab.Post(key);
            }

            ab.Complete();
            await ab.Completion;

            async Task LocalAsync(string k)
            {
                try
                {
                    await db.KeyDeleteAsync(k, CommandFlags.DemandMaster);
                    Trace.WriteLine(k); 
                }
                #region Exception Handling

                catch (RedisTimeoutException ex)
                {
                    _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
                }
                catch (Exception ex)
                {
                    _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
                }

                #endregion // Exception Handling
            }
        }

        #endregion // DELETE_KEYS_TEST
    }
}
