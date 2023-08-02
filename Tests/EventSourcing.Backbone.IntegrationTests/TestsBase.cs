using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

using EventSourcing.Backbone.IntegrationTests.HelloWorld;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Xunit.Abstractions;

using static EventSourcing.Backbone.Channels.RedisProvider.Common.RedisChannelConstants;

#pragma warning disable S3881 // "IDisposable" should be implemented correctly

// docker run -p 6379:6379 -it --rm --name redis-evt-src redislabs/rejson:latest

namespace EventSourcing.Backbone.Tests
{
    /// <summary>
    /// The end to end tests.
    /// </summary>
    public abstract class TestsBase : IDisposable
    {
        protected readonly ITestOutputHelper _outputHelper;
        protected readonly ILogger _fakeLogger = A.Fake<ILogger>();
        protected const int TIMEOUT = 1_000 * 50;

        protected abstract string URI { get; }

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloWorldTests"/> class.
        /// </summary>
        /// <param name="outputHelper">The output helper.</param>
        protected TestsBase(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        #endregion // Ctor

        #region Dispose pattern


        ~TestsBase()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                IConnectionMultiplexer conn = RedisClientFactory.CreateProviderAsync(
                                                    logger: _fakeLogger,
                                                    configurationHook: cfg => cfg.AllowAdmin = true).Result;
                string serverNames = Environment.GetEnvironmentVariable(END_POINT_KEY) ?? "localhost:6379";
                foreach (var serverName in serverNames.Split(','))
                {
                    var server = conn.GetServer(serverName);
                    if (!server.IsConnected)
                        continue;
                    IEnumerable<RedisKey> keys = server.Keys(pattern: $"*{URI}*");
                    IDatabaseAsync db = conn.GetDatabase();

                    var ab = new ActionBlock<string>(k => LocalAsync(k), new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 30 });
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                    foreach (string key in keys)
                    {
                        ab.Post(key);
                    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    ab.Complete();
                    ab.Completion.Wait();

                    async Task LocalAsync(string k)
                    {
                        try
                        {
                            await db.KeyDeleteAsync(k, CommandFlags.DemandMaster);
                            _outputHelper.WriteLine($"Cleanup: delete key [{k}]");
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
            }
            #region Exception Handling

            catch (RedisTimeoutException ex)
            {
                _outputHelper.WriteLine($"Test dispose timeout error (delete keys) {ex.FormatLazy()}");
            }
            catch (Exception ex)
            {
                _outputHelper.WriteLine($"Test dispose error (delete keys) {ex.FormatLazy()}");
            }

            #endregion // Exception Handling
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        #endregion // Dispose pattern

        #region GetCancellationToken

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <returns></returns>
        protected static CancellationToken GetCancellationToken(int prodSec = 10)
        {
            return new CancellationTokenSource(Debugger.IsAttached
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromSeconds(prodSec)).Token;
        }

        #endregion // GetCancellationToken

    }
}
