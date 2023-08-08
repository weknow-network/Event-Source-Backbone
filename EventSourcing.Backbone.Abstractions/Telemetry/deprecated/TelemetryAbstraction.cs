//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using OpenTelemetry.Context.Propagation;

//using OpenTelemetry;

//namespace EventSourcing.Backbone.Telemetry;

///// <summary>
///// Telemetry api
///// </summary>
//public class TelemetryAbstraction: ITelemetryAbstraction
//{
//    internal static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

//    /// <summary>
//    /// Telemetry operators
//    /// </summary>
//    private readonly ActivitySource _telemetry;

//    public TelemetryAbstraction(string source)
//    {
//        _telemetry = new ActivitySource(source);
//    }

//    /// <summary>
//    /// Starts a trace.
//    /// </summary>
//    /// <param name="name">The name.</param>
//    /// <param name="kind">The kind.</param>
//    /// <returns></returns>
//    private Activity? Start([CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal)
//    {
//        return _telemetry.StartActivity(name, kind);
//    }

//    /// <summary>
//    /// Starts a trace.
//    /// </summary>
//    /// <param name="name">The name.</param>
//    /// <param name="kind">The kind.</param>
//    /// <param name="parentContext">The parent context.</param>
//    /// <param name="tags">The tags.</param>
//    /// <param name="links">The links.</param>
//    /// <param name="startTime">The start time.</param>
//    /// <returns></returns>
//    private Activity? Start(
//        string name,
//        ActivityKind kind,
//        ActivityContext parentContext,
//        IEnumerable<KeyValuePair<string, object?>>? tags = null,
//        IEnumerable<ActivityLink>? links = null,
//        DateTimeOffset startTime = default)
//    {
//        return _telemetry.StartActivity(name, kind, parentContext, tags, links, startTime);
//    }

//    /// <summary>
//    /// Starts a trace.
//    /// </summary>
//    /// <param name="name">The name.</param>
//    /// <param name="kind">The kind.</param>
//    /// <param name="parentId">The parent identifier.</param>
//    /// <param name="tags">The tags.</param>
//    /// <param name="links">The links.</param>
//    /// <param name="startTime">The start time.</param>
//    /// <returns></returns>
//    private Activity? Start(
//        string name,
//        ActivityKind kind,
//        string? parentId,
//        IEnumerable<KeyValuePair<string, object?>>? tags = null,
//        IEnumerable<ActivityLink>? links = null,
//        DateTimeOffset startTime = default)

//    {
//        return _telemetry.StartActivity(name, kind, parentId, tags, links, startTime);
//    }

//    private Activity? Start(
//        ActivityKind kind,
//        ActivityContext parentContext = default,
//        IEnumerable<KeyValuePair<string, object?>>? tags = null,
//        IEnumerable<ActivityLink>? links = null,
//        DateTimeOffset startTime = default,
//        [CallerMemberName] string name = "")

//    {
//        return _telemetry.StartActivity(kind, parentContext, tags, links, startTime, name);
//    }
//}
