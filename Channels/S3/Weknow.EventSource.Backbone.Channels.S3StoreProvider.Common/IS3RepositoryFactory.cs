using System;

namespace Weknow.EventSource.Backbone.Channels
{
    public interface IS3RepositoryFactory
    {
        S3Repository Get(string bucket, string? basePath = null);
    }
}