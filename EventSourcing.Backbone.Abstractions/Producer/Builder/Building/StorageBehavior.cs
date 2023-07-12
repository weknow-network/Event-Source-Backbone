namespace EventSourcing.Backbone;

/// <summary>
/// Tune the storage behavior
/// </summary>
public record StorageBehavior
{
    public static readonly StorageBehavior Empty = new StorageBehavior();

    /// <summary>
    /// Useful when having multi storage configuration.
    /// May use to implement storage splitting (separation of concerns) like in the case of GDPR.
    /// The predicate signature is: (metadata, key) => bool
    ///   the key is driven from the method parameter.
    /// </summary>
    public Func<Metadata, string, bool>? Filter { get; init; }

    /// <summary>
    /// Specialized the storage to address a specific target category.
    /// </summary>
    public EventBucketCategories Category { get; init; } = EventBucketCategories.All;

    /// <summary>
    /// Time to live (TTL) which will be attached to each entity.
    /// BE CAREFUL, USE IT WHEN THE STORAGE USE AS CACHING LAYER!!!
    /// Setting this property to no-null value will make the storage ephemeral.
    /// </summary>
    public TimeSpan? timeToLive { get; init; } 

    #region Cast overloads

    /// <summary>
    /// Performs an implicit conversion from <see cref="EventBucketCategories"/> to <see cref="StorageBehavior"/>.
    /// </summary>
    /// <param name="castFrom">The cast from.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator StorageBehavior(EventBucketCategories castFrom)
    {
        return new StorageBehavior
        {
            Category = castFrom
        };
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="TimeSpan"/> to <see cref="StorageBehavior"/>.
    /// </summary>
    /// <param name="castFrom">The cast from.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator StorageBehavior(TimeSpan castFrom)
    {
        return new StorageBehavior
        {
            timeToLive = castFrom
        };
    }

    /// <summary>
    /// Performs an implicit conversion/>.
    /// </summary>
    /// <param name="castFrom">The cast from.</param>
    /// <returns>
    /// The result of the conversion.
    /// </returns>
    public static implicit operator StorageBehavior(Func<Metadata, string, bool>? castFrom)
    {
        return new StorageBehavior
        {
            Filter = castFrom
        };
    }

    #endregion // Cast overloads
}
