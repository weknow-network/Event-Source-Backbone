using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Weknow.EventSource.Backbone
{
    public static class TelemetryrExtensions
    {
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        #region StartSpanScope

        /// <summary>
        /// Starts telemetry span scope.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activitySource">The activity source.</param>
        /// <param name="meta">The meta.</param>
        /// <param name="entriesBuilder">The entries builder.</param>
        /// <param name="injectStrategy">The injection strategy.</param>
        /// <returns></returns>
        public static Activity? StartSpanScope<T>(
                        this ActivitySource activitySource,
                        Metadata meta,
                        ImmutableArray<T>.Builder entriesBuilder,
                        Action<ImmutableArray<T>.Builder, string, string> injectStrategy)
        {
            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
            var activityName = $"{meta.Operation} send";
            Activity? activity = activitySource.StartActivity(activityName, ActivityKind.Producer);
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


            // Inject the ActivityContext into the message metadata to propagate trace context to the receiving service.
            Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), entriesBuilder, injectStrategy);

            // The OpenTelemetry messaging specification defines a number of attributes. These attributes are added here.
            meta.InjectConnonTelemetryTags(activity);
            return activity;
        }

        #endregion // StartSpanScope

        #region InjectConnonTelemetryTags

        /// <summary>
        /// Adds standard open-telemetry tags (for redis).
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="activity">The activity.</param>
        private static void InjectConnonTelemetryTags(this Metadata meta, Activity? activity)
        {
            // These tags are added demonstrating the semantic conventions of the OpenTelemetry messaging specification
            // See:
            //   * https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#messaging-attributes
            activity?.SetTag("messaging.destination", meta.Operation);
            activity?.SetTag("messaging.message_id", meta.MessageId);
        }

        #endregion // InjectConnonTelemetryTags
    }
}
