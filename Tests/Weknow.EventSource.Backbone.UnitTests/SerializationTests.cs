using FakeItEasy;

using System;
using System.Threading.Channels;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;
using Weknow.EventSource.Backbone.UnitTests.Entities;

using Xunit;
using Xunit.Abstractions;



namespace Weknow.EventSource.Backbone
{
    public class SerializationTests
    {
        #region Metadata_Serialization_Test

        [Fact]
        public void Metadata_Serialization_Test()
        {
            var meta = new Metadata
            {
                MessageId = "message1",
                Environment = "env1",
                Partition = "partition1",
                Shard = "shard1",
                Operation = "operation1"
            };

            var serializer = new EventSourceOptions().Serializer;

            var buffer = serializer.Serialize(meta);
            var deserialize = serializer.Deserialize<Metadata>(buffer);

            Assert.Equal(meta, deserialize);
        }

        #endregion // Metadata_Serialization_Test
    }
}
