using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;

using EventSourcing.Backbone;

namespace WebSampleS3;

public static class Constants
{
    public const string URI = "hello-event-sourcing";
    public const string S3_BUCKET = "event-sourcing-demo";

    public static IAmazonS3 CreateS3Client(RegionEndpoint region, string profile = "playground")
    {
        var chain = new CredentialProfileStoreChain();
        AWSCredentials awsCredentials;
        if (chain.TryGetAWSCredentials(profile, out awsCredentials))
        {
            // Use awsCredentials to create an Amazon S3 service client
            var client = new AmazonS3Client(awsCredentials, region);
            return client;
        }

        var s3Client = EventSourcing.Backbone.Channels.S3RepositoryFactory.CreateClient();
        return s3Client;
    }

    public static readonly S3Options S3Options = new S3Options
    {
        Bucket = Constants.S3_BUCKET
    };

}
