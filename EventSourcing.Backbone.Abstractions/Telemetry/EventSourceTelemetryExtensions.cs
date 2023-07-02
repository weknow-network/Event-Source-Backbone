using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

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
        activity?.SetTag("evt-src.operation", meta.Operation);
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

        var levels = plan.Options.TelemetryLevel;
        activity?.SetTag("evt-src.env", plan.Environment);
        activity?.SetTag("evt-src.uri", plan.Uri);

        //activity?.SetTag("evt-src.trace-level", levels.Trace);
        //activity?.SetTag("evt-src.metrics-level", levels.Metric);

        if (meta != null)
        {
            activity?.SetTag("evt-src.operation", meta.Operation);
            activity?.SetTag("evt-src.message-id", meta.MessageId);
            activity?.SetTag("evt-src.channel-type", meta.ChannelType);
        }
    }

    #endregion // InjectTelemetryTags

    #region StartInternalTraceCritical

    /// <summary>
    /// Starts a trace Critical level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceCritical(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Critical) 
            return null;

        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceCritical

    #region StartInternalTraceError

    /// <summary>
    /// Starts a trace Error level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceError(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Error) 
            return null;

        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceError

    #region StartInternalTraceWarning

    /// <summary>
    /// Starts a trace warning level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceWarning(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Warning) 
            return null;

        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceWarning

    #region StartInternalTraceInformation

    /// <summary>
    /// Starts a trace Information level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceInformation(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Information) 
            return null;

        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceInformation

    #region StartInternalTraceOnTraceLevel

    /// <summary>
    /// Starts a trace on Trace level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceOnTraceLevel(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Trace)
            return null;
        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceOnTraceLevel

    #region StartInternalTraceDebug

    /// <summary>
    /// Starts a trace Debug level.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    public static Activity? StartInternalTraceDebug(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        if (plan.Options.TelemetryLevel.Trace > LogLevel.Debug)
            return null;
        var activity = activitySource.StartInternalTrace(plan, name, tagsAction, metadata);
        return activity;
    }

    #endregion // StartInternalTraceDebug

    #region StartInternalTrace

    /// <summary>
    /// Starts a trace.
    /// </summary>
    /// <param name="activitySource">The activity source.</param>
    /// <param name="plan">The plan.</param>
    /// <param name="name">The name.</param>
    /// <param name="tagsAction">The tags action.</param>
    /// <param name="metadata">The metadata.</param>
    /// <returns></returns>
    private static Activity? StartInternalTrace(this ActivitySource activitySource,
                                        IPlanBase plan,
                                        string name,
                                        Action<ITagAddition>? tagsAction = null,
                                        Metadata? metadata = null)
    {
        // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
        // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name

        var tags = new ActivityTagsCollection();
        var t = new TagAddition(tags);
        tagsAction?.Invoke(t);

        ActivityContext? context = Activity.Current?.Context;
        Activity? activity;
        if (context == null)
        {
            activity = activitySource.StartActivity(
                                                    name,
                                                    ActivityKind.Internal,
                                                    Activity.Current?.Context ?? default, tags);
        }
        else
        {
            var link = new ActivityLink((ActivityContext)context);
            activity = activitySource.StartActivity(
                                                    name,
                                                    ActivityKind.Internal,
                                                    Activity.Current?.Context ?? default, tags, link.ToEnumerable());
        }
        plan.InjectTelemetryTags(activity, metadata);


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
