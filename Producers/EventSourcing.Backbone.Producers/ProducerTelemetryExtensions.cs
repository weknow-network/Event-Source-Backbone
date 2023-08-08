using System.Diagnostics;

using Microsoft.Extensions.Logging;

using static EventSourcing.Backbone.Private.EventSourceTelemetry;

namespace EventSourcing.Backbone.Producers;

public static class ProducerTelemetryExtensions
{
    #region StartProducerTrace

    /// <summary>
    /// Starts a consumer trace.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="meta">The metadata.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <returns></returns>
    public static Activity? StartProducerTrace(this IProducerPlan plan,
                                        Metadata meta,
                                        Action<ITagAddition>? tagsAction = null,
                                        ActivitySource? activitySource = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name

        activitySource = activitySource ?? ETracer;
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Debug)
            return null;

        if (!activitySource.HasListeners())
            return null;

        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);


        var activityName = activitySource.HasListeners() ? $"producer.{meta.Signature.Operation.ToDash()}.send" : string.Empty;
        Activity? activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
        plan.InjectTelemetryTags(activity, meta);

        return activity;
    }

    #endregion // StartProducerTrace
}
