// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.Json;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

using Weknow.EventSource.Backbone;
using Weknow.EventSource.Backbone.Building;

const string TARGET_KEY = "REDIS_MIGRATION_TARGET_ENDPOINT";
const string ENV = "Production";
const string PARTITION = "analysts";
const string SHARD = "default";
const string CHANNEL_TYPE = "REDIS Channel V1";
const string PROVIDER_ID = "S3_V1";

var producer = ProducerBuilder.Empty
                    .UseRedisChannel(credentialsKeys: new RedisCredentialsKeys { EndpointKey = TARGET_KEY })
                    .AddVoidStrategy(PROVIDER_ID)
                    //.AddS3Strategy(options: new S3Options {  DryRun = true})
                    .Environment(ENV)
                    .Partition(PARTITION)
                    .Shard(SHARD)
                    .BuildRaw(new RawProducerOptions { KeepOriginalMeta = true });


string accessKey =
    Environment.GetEnvironmentVariable("S3_EVENT_SOURCE_ACCESS_KEY") ?? "";
string secretKey =
    Environment.GetEnvironmentVariable("S3_EVENT_SOURCE_SECRET") ?? "";
string? regionKey =
    Environment.GetEnvironmentVariable("S3_EVENT_SOURCE_REGION");
RegionEndpoint rgnKey = (!string.IsNullOrEmpty(regionKey))
                            ? RegionEndpoint.GetBySystemName(regionKey)
                            : RegionEndpoint.USEast2;

var s3 = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    rgnKey);

ListObjectsV2Response list = await s3.ListObjectsV2Async(new ListObjectsV2Request { BucketName = "prod.event-source-storage", Prefix = "analysts" });

var groups = list.S3Objects.Select(m =>
            {
                string[] parts = m.Key.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var key = parts[^1];
                string value = m.Key; // .Replace("\"", "").Trim();
                return new { Group = parts[5], key, value};
            })
            .GroupBy(m => m.Group, m => (m.key, Encoding.UTF8.GetBytes(m.value)));
foreach (var group in groups)
{
    Console.WriteLine($"---------- {group.Key} --------");

    var pairs = group.ToArray();
    Bucket segments = Bucket.Empty.AddRange(pairs);


    var announcement = new Announcement
    {
        Metadata = new Metadata
        {
            MessageId = group.Key,
            ChannelType = CHANNEL_TYPE,
            Operation = "AnalystOnboardAsync",
            Origin = MessageOrigin.Copy,
            Environment = ENV,
            Partition = PARTITION,
            Shard = SHARD,
        },
        Segments = segments
    };
    await producer.Produce(announcement);
}

