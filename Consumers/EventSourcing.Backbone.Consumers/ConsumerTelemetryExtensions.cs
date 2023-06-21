using System.Collections;
using System.Diagnostics;

namespace EventSourcing.Backbone.Consumers;

public static class ConsumerTelemetryExtensions
{
    #region StartConsumerTrace

    /// <summary>
    /// Starts a consumer trace.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="meta">The metadata.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <returns></returns>
    public static Activity? StartConsumerTrace(this ActivitySource activitySource,
                                        Metadata meta,
                                        ActivityContext parentContext,
                                        Action<ITagAddition>? tagsAction = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        var activityName = $"event-source.Consumer.{meta.Operation}.process";

        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);

        Activity? activity = activitySource.StartActivity(
                                                activityName,
                                                ActivityKind.Consumer,
                                                parentContext, tags, links: new ActivityLink(parentContext).ToEnumerable());
        meta.InjectTelemetryTags(activity);

        return activity;
    }

    #endregion // StartConsumerTrace
}
