using System.Text.Json;

using Xunit;

namespace EventSourcing.Backbone
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
                Uri = "uri1",
                Operation = "operation1"
            };

            var serializer = new EventSourceOptions().Serializer;

            var buffer = serializer.Serialize(meta);
            var deserialize = serializer.Deserialize<Metadata>(buffer);

            Assert.Equal(meta, deserialize);
        }

        #endregion // Metadata_Serialization_Test

        #region Bucket_Serialization_Test

        [Fact]
        public void Bucket_Serialization_Test()
        {
            var backet = Bucket.Empty.Add("X", new byte[] { 1, 2 });

            var serializer = new EventSourceOptions().Serializer;

            var buffer = serializer.Serialize(backet);
            var deserialize = serializer.Deserialize<Bucket>(buffer);

            Assert.True(deserialize.TryGetValue("X", out var arr));
            Assert.Equal(1, arr.Span[0]);
            Assert.Equal(2, arr.Span[1]);
        }

        #endregion // Bucket_Serialization_Test

        #region Announcement_Serialization_Test

        [Fact]
        public void Announcement_Serialization_Test()
        {
            var meta = new Metadata
            {
                MessageId = "message1",
                Environment = "env1",
                Uri = "uri1",
                Operation = "operation1"
            };
            meta = meta with { Linked = meta, Origin = MessageOrigin.Copy };
            var announcement = new Announcement
            {
                Metadata = meta,
                Segments = Backbone.Bucket.Empty.Add("X", new byte[] { 1, 2 })
            };

            var serializer = new EventSourceOptions().Serializer;

            var buffer = serializer.Serialize(announcement);
            var deserialize = serializer.Deserialize<Announcement>(buffer);

            Assert.Equal(announcement.Metadata, deserialize.Metadata);
            Assert.True(announcement.Segments.TryGetValue("X", out var arr));
            Assert.True(deserialize.Segments.TryGetValue("X", out arr));
            Assert.Equal(1, arr.Span[0]);
            Assert.Equal(2, arr.Span[1]);
        }

        #endregion // Announcement_Serialization_Test
    }
}
