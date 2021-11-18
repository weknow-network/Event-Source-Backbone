using System;

namespace Weknow.EventSource.Backbone.Channels
{
    public interface IS3RepositoryFactory
    {
        /// <summary>
        /// Get repository
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        S3Repository Get(S3Options options = default);
    }
}