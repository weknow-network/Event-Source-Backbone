using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using static EventSourcing.Backbone.Private.EventSourceTelemetry;

namespace EventSourcing.Backbone;

public static class EventSourceTelemetryExtensions
{
    internal static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    #region InjectSpan

    /// <summary>
    /// Inject telemetry span to the channel property (Before sending).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="activity">The activity.</param>
    /// <param name="entriesBuilder">The entries builder.</param>
    /// <param name="injectStrategy">The injection strategy.</param>
    /// <returns></returns>
    public static void InjectSpan<T>(
                    this Activity? activity,
                    ImmutableArray<T>.Builder entriesBuilder,
                    Action<ImmutableArray<T>.Builder, string, string> injectStrategy)
    {
        // Depending on Sampling (and whether a listener is registered or not), the
        // activity above may not be created.
        // If it is created, then propagate its context.
        // If it is not created, the propagate the Current context,
        // if any.
        if (activity != null)
        {
            Activity.Current = activity;
        }
        ActivityContext contextToInject = Activity.Current?.Context ?? default;

        // Inject the ActivityContext
        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), entriesBuilder, injectStrategy);
    }

    #endregion // InjectSpan

    #region ExtractSpan

    /// <summary>
    /// Extract telemetry span's parent info (while consuming)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entries">The entries for extraction.</param>
    /// <param name="injectStrategy">The injection strategy.</param>
    /// <returns></returns>
    public static ActivityContext ExtractSpan<T>(
                    T entries,
                    Func<T, string, IEnumerable<string>> injectStrategy)
    {
        PropagationContext parentContext = Propagator.Extract(default, entries, injectStrategy);
        Baggage.Current = parentContext.Baggage;

        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
        return parentContext.ActivityContext;
    }

    #endregion // ExtractSpan

    #region InjectTelemetryTags

    /// <summary>
    /// Adds standard open-telemetry tags (for redis).
    /// </summary>
    /// <param name="meta">The meta.</param>
    /// <param name="activity">The activity.</param>
    public static void InjectTelemetryTags(this Metadata meta, Activity? activity)
    {
        // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
        // See:
        //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes

        activity?.SetTag("evt-src.env", meta.Environment);
        activity?.SetTag("evt-src.uri", meta.Uri);
        activity?.SetTag("evt-src.operation", meta.Signature);
        activity?.SetTag("evt-src.message-id", meta.MessageId);
        activity?.SetTag("evt-src.channel-type", meta.ChannelType);
    }

    /// <summary>
    /// Adds standard open-telemetry tags (for redis).
    /// </summary>
    /// <param name="plan">The builder plan.</param>
    /// <param name="activity">The activity.</param>
    /// <param name="meta">The metadata.</param>
    public static void InjectTelemetryTags(this IPlanBase plan, Activity? activity, Metadata? meta = null)
    {
        // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
        // See:
        //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes

        activity?.SetTag("evt-src.env", plan.Environment);
        activity?.SetTag("evt-src.uri", plan.Uri);

        if (meta != null)
        {
            var signature = meta.Signature;
            activity?.SetTag("evt-src.operation", signature.Operation);
            activity?.SetTag("evt-src.version", signature.Version);
            activity?.SetTag("evt-src.params", signature.Parameters);
            activity?.SetTag("evt-src.message-id", meta.MessageId);
            activity?.SetTag("evt-src.channel-type", meta.ChannelType);
        }
    }

    #endregion // InjectTelemetryTags 

    #region StartTrace

    /// <summary>
    /// Starts the trace error.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="kind">The kind.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTrace(this string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        ActivityKind kind = ActivityKind.Internal,
                                        Metadata? metadata = null)
    {
        var activity = StartInternalTrace(name, null, null, tagsAction, metadata, kind);
        return activity;
    }

    /// <summary>
    /// Starts the trace error.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <param name="kind">The kind.</param>
    /// <returns></returns>
    public static Activity? StartTrace(this ActivitySource activitySource,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null,
                                        ActivityKind kind = ActivityKind.Internal)
    {
        var activity = StartInternalTrace(name, activitySource, null, tagsAction, metadata, kind);
        return activity;
    }

    #endregion // StartTrace

    #region StartTraceCritical

    /// <summary>
    /// Starts the trace error.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceCritical(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Critical)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceCritical

    #region StartTraceError

    /// <summary>
    /// Starts the trace error.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceError(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Error)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceError

    #region StartTraceWarning

    /// <summary>
    /// Starts the trace warning.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceWarning(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Warning)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceWarning

    #region StartTraceInformation

    /// <summary>
    /// Starts the trace information.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceInformation(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Information)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceInformation

    #region StartTraceDebug

    /// <summary>
    /// Starts the trace debug.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceDebug(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Debug)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceDebug

    #region StartTraceVerbose

    /// <summary>
    /// Starts the trace verbose.
    /// </summary>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartTraceVerbose(this IPlanBase plan,
                                        string name,
                                        ActivitySource? activitySource = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Trace)
            return null;
        var activity = StartInternalTrace(name, activitySource, plan, tagsAction, metadata);
        return activity;
    }

    #endregion // StartTraceVerbose

    #region StartInternalTrace

    /// <summary>
    /// Starts a trace.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <param name="kind">The kind.</param>
    /// <returns></returns>
    private static Activity? StartInternalTrace(string name,
                                        ActivitySource? activitySource = null,
                                        IPlanBase? plan = null,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null,
                                        ActivityKind kind = ActivityKind.Internal)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name

        activitySource = activitySource ?? ETracer;
        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);

        ActivityContext? context = Activity.Current?.Context;
        Activity? activity;
        if (context == null)
        {
            activity = activitySource.StartActivity(
                                                    name,
                                                    kind,
                                                    Activity.Current?.Context ?? default, tags);
        }
        else
        {
            var link = new ActivityLink((ActivityContext)context);
            activity = activitySource.StartActivity(
                                                    name,
                                                    kind,
                                                    Activity.Current?.Context ?? default, tags, link.ToEnumerable());
        }
        plan?.InjectTelemetryTags(activity, metadata);


        return activity;
    }

    #endregion // StartInternalTrace

    #region AddEvent

    /// <summary>
    /// Adds the event.
    /// </summary>
    /// <param name="current">The current.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <returns></returns>
    public static Activity? AddEvent(this Activity current, string name, Action<ITagAddition>? tagsAction = null)
    {
        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);
        var e = new ActivityEvent(name, tags: tags);
        return current?.AddEvent(e);
    }

    #endregion // AddEvent
}
