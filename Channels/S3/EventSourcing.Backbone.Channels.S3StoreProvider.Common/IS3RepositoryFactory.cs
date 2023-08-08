namespace EventSourcing.Backbone.Channels
{
    public interface IS3RepositoryFactory
    {
        /// <summary>
        /// Get repository
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        IS3Repository Get(S3Options? options = null);
    }
}