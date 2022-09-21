using FakeItEasy;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        #region Announcement_Serialization_Test

        [Fact]
        public void Announcement_Serialization_Test()
        {
            var meta = new Metadata
            {
                MessageId = "message1",
                Environment = "env1",
                Partition = "partition1",
                Shard = "shard1",
                Operation = "operation1"
            };
            meta = meta with { Linked = meta, Origin = MessageOrigin.Copy };
            var announcement = new Announcement
            {
                Metadata = meta,
                Segments = Bucket.Empty.Add("X", new byte[] { 1, 2 })
            };

            var serializer = new EventSourceOptions().Serializer;

            var buffer = serializer.Serialize(announcement);
            var deserialize = serializer.Deserialize<Announcement>(buffer);

            Assert.Equal(announcement.Metadata, deserialize.Metadata);
            Assert.True(deserialize.Segments.TryGetValue("X", out var arr));
            Assert.Equal(1, arr.Span[0]) ;
            Assert.Equal(2, arr.Span[1]) ;
        }

        #endregion // Announcement_Serialization_Test

        #region // Announcement_Serialization_Gen_Test

        //[Fact]
        //public void Announcement_Serialization_Gen_Test()
        //{
        //    var meta = new Metadata
        //    {
        //        MessageId = "message1",
        //        Environment = "env1",
        //        Partition = "partition1",
        //        Shard = "shard1",
        //        Operation = "operation1"
        //    };
        //    meta = meta with { Linked = meta, Origin = MessageOrigin.Copy };
        //    var announcement = new Announcement
        //    {
        //        Metadata = meta,
        //        Segments = Bucket.Empty.Add("X", new byte[] { 1, 2 })
        //    };

        //    var serializer = new EventSourceOptions().Serializer;

        //    var buffer = JsonSerializer.SerializeToUtf8Bytes(announcement, JsonContext.Default.Announcement);
        //    var deserialize = JsonSerializer.Deserialize(buffer, JsonContext.Default.Announcement);

        //    Assert.Equal(announcement, deserialize);
        //}

        #endregion // Announcement_Serialization_Gen_Test
    }
}
