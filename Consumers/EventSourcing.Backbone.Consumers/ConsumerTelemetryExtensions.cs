using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EventSourcing.Backbone.Consumers;

public static class ConsumerTelemetryExtensions
{
    #region StartConsumerTrace

    /// <summary>
    /// Starts a consumer trace.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="meta">The metadata.</param>
    /// <param name="parentContext">The parent context.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <returns></returns>
    public static Activity? StartConsumerTrace(this ActivitySource activitySource,
                                        IConsumerPlanBase plan,
                                        Metadata meta,
                                        ActivityContext parentContext,
                                        Action<ITagAddition>? tagsAction = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        var activityName = $"consumer.{meta.Operation}.process";

        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);

        Activity? activity = activitySource.StartActivity(
                                                activityName,
                                                ActivityKind.Consumer,
                                                parentContext, tags, links: new ActivityLink(parentContext).ToEnumerable());
        plan.InjectTelemetryTags(activity, meta);

        return activity;
    }

    #endregion // StartConsumerTrace

    #region WithEnvUriOperation

    /// <summary>
    /// Add URI, Environment and Operation labels.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="counter">The counter.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static ICounterBuilder<T> WithEnvUriOperation<T>(this Counter<T> counter, Metadata metadata)
        where T : struct
    {
        return counter
                    .WithTag("uri", metadata.UriDash)
                    .WithTag("env", metadata.Environment.DashFormat())
                    .WithTag("operation", metadata.Operation.ToDash());
    }

    /// <summary>
    /// Add URI, Environment and Operation labels.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="counter">The counter.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static ICounterBuilder<T> WithEnvUriOperation<T>(this ICounterBuilder<T> counter, Metadata metadata)
        where T : struct
    {
        return counter
                    .WithTag("uri", metadata.UriDash)
                    .WithTag("env", metadata.Environment.DashFormat())
                    .WithTag("operation", metadata.Operation.ToDash());
    }

    #endregion // WithEnvUriOperation

    #region AddEnvUriOperation

    /// <summary>
    /// Add URI, Environment and Operation labels.
    /// </summary>
    /// <param name="counter">The counter.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static ITagAddition AddEnvUriOperation(this ITagAddition counter, Metadata metadata)
    {
        return counter
                    .Add("uri", metadata.UriDash)
                    .Add("env", metadata.Environment.DashFormat())
                    .Add("operation", metadata.Operation.ToDash());
    }

    #endregion // AddEnvUriOperation
}
