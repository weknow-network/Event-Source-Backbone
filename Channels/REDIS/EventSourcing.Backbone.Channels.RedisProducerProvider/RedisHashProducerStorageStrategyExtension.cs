using EventSourcing.Backbone.Channels;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

namespace EventSourcing.Backbone;

/// <summary>
/// Extension methods for S3 storage strategy.
/// </summary>
public static class RedisHashProducerStorageStrategyExtension
{
    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="credential">The credential.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="configurationHook">The configuration hook.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerStoreStrategyBuilder AddRedisHashStorage(
            this IProducerStoreStrategyBuilder builder,
            IRedisCredentials credential,
            Predicate<string>? filter = null,
            EventBucketCategories targetType = EventBucketCategories.All,
            Action<ConfigurationOptions>? configurationHook = null,
            TimeSpan? timeToLive = null)
    {

        var result = builder.AddStorageStrategy(Local, targetType, filter);
        return result;

        IProducerStorageStrategy Local (ILogger logger)
        {
            var configuration = credential.CreateConfigurationOptions(configurationHook);
            var connFactory = EventSourceRedisConnectionFactory.Create(logger, configuration);
            var storage = new RedisHashStorageStrategy (connFactory, logger, timeToLive);
            return storage;
        }
    }

    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerStoreStrategyBuilder AddRedisHashStorage(
            this IProducerStoreStrategyBuilder builder,
            Predicate<string>? filter = null,
            EventBucketCategories targetType = EventBucketCategories.All,
            TimeSpan? timeToLive = null)
    {
        var result = builder.AddStorageStrategy(Local, targetType, filter);
        return result;

        IProducerStorageStrategy Local (ILogger logger)
        {
            var connFactory = EventSourceRedisConnectionFactory.Create(logger);
            var storage = new RedisHashStorageStrategy (connFactory, logger, timeToLive);
            return storage;
        }
    }

    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerStoreStrategyBuilder AddRedisHashStorage(
            this IProducerStoreStrategyBuilder builder,
            ConfigurationOptions configuration,
            Predicate<string>? filter = null,
            EventBucketCategories targetType = EventBucketCategories.All,
            TimeSpan? timeToLive = null)
    {

        var result = builder.AddStorageStrategy(Local, targetType, filter);
        return result;

        IProducerStorageStrategy Local (ILogger logger)
        {
            var connFactory = EventSourceRedisConnectionFactory.Create(logger, configuration);
            var storage = new RedisHashStorageStrategy (connFactory, logger, timeToLive);
            return storage;
        }
    }

    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="connFactory">The redis connection factory.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerStoreStrategyBuilder AddRedisHashStorage(
        this IProducerStoreStrategyBuilder builder,
        IEventSourceRedisConnectionFactory connFactory,
        Predicate<string>? filter = null,
        EventBucketCategories targetType = EventBucketCategories.All,
            TimeSpan? timeToLive = null)
    {

        var result = builder.AddStorageStrategy(Local, targetType, filter);
        return result;

        IProducerStorageStrategy Local (ILogger logger)
        {
            var storage = new RedisHashStorageStrategy (connFactory, logger, timeToLive);
            return storage;
        }
    }

    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerIocStoreStrategyBuilder ResolveRedisHashStorage(
            this IProducerIocStoreStrategyBuilder builder,
            Predicate<string> filter,
            TimeSpan? timeToLive = null)
    {
        var result = builder.ResolveRedisHashStorage(EventBucketCategories.All, filter, timeToLive);
        return result;
    }

    /// <summary>
    /// Resolves the redis hash.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="targetType">Type of the target.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="timeToLive">
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </param>
    /// <returns></returns>
    public static IProducerIocStoreStrategyBuilder ResolveRedisHashStorage(
            this IProducerIocStoreStrategyBuilder builder,
            EventBucketCategories targetType = EventBucketCategories.All,
            Predicate<string>? filter = null,
            TimeSpan? timeToLive = null)
    {

        var result = builder.AddStorageStrategy(Local, targetType, filter);
        return result;

        IProducerStorageStrategy Local(ILogger logger)
        {
            IEventSourceRedisConnectionFactory? connFactory = builder.ServiceProvider.GetService<IEventSourceRedisConnectionFactory>();
            if (connFactory == null)
            {
                string error = $"Redis Hash Storage: {nameof(IEventSourceRedisConnectionFactory)} is not registered as dependency injection";
                logger.LogError(error);
                throw new EventSourcingException(error);
            }
            var storage = new RedisHashStorageStrategy(connFactory, logger, timeToLive);
            return storage;
        }
    }
}
