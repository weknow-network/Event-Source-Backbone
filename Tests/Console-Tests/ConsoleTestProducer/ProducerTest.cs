using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FakeItEasy;

using Microsoft.Extensions.Logging;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

using static Weknow.EventSource.Backbone.ConsoleTests.Constants;


namespace Weknow.EventSource.Backbone.ConsoleTests
{
    static class ProducerTest
    {
        static async Task Main(string[] args)
        {
            var producerBuilder = ProducerBuilder.Empty.UseRedisProducerChannel(
                                        CancellationToken.None,
                                        configuration: (cfg) => cfg.ServiceName = "mymaster");

            while (true)
            {

                Console.WriteLine("Press any key for PRODUCE");
                Console.ReadKey(true);

                #region ISequenceOperations producer = ...

                ISequenceOperations producer = producerBuilder
                                                //.WithOptions(producerOption)
                                                .Partition(PARTITION)
                                                .Shard(SHARD_A)
                                                .Build<ISequenceOperations>();

                #endregion // ISequenceOperations producer = ...

                await SendSequenceAsync(producer);
            }
        }

        #region SendSequenceAsync

        /// <summary>
        /// Sends standard test sequence.
        /// </summary>
        /// <param name="producer">The producer.</param>
        private static async Task SendSequenceAsync(
            ISequenceOperations producer)
        {
            int id = (Environment.TickCount % int.MaxValue);
            await producer.RegisterAsync(new User($"User {Environment.TickCount % 1000}", $"ID{id}"));
            await producer.LoginAsync("admin", "1234");
            await producer.EarseAsync(id);
        }

        #endregion // SendSequenceAsync


    }
}
