using System.Diagnostics;

namespace EventSourcing.Backbone.Producers;

public static class ProducerTelemetryExtensions
{
    #region StartProducerTrace

    /// <summary>
    /// Starts a consumer trace.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="meta">The metadata.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <returns></returns>
    public static Activity? StartProducerTrace(this ActivitySource activitySource,
                                        Metadata meta,
                                        Action<ITagAddition>? tagsAction = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name

        if(!activitySource.HasListeners())
            return null;

        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);


        var activityName = activitySource.HasListeners() ? $"evt-src.producer.{meta.Environment.ToDash()}.{meta.UriDash}.{meta.Operation.ToDash()}.send" : string.Empty;
        Activity? activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
        meta.InjectTelemetryTags(activity);

        return activity;
    }

    #endregion // StartProducerTrace
}
