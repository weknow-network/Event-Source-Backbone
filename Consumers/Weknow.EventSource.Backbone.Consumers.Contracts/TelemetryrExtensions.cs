using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using System;
using System.Collections.Generic;
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
        /// <param name="entries">The entries for extraction.</param>
        /// <param name="injectStrategy">The injection strategy.</param>
        /// <returns></returns>
        public static Activity? StartSpanScope<T>(
                        this ActivitySource activitySource,
                        Metadata meta,
                        T entries,
                        Func<T, string, IEnumerable<string>> injectStrategy)
        {
            PropagationContext parentContext = Propagator.Extract(default, entries, injectStrategy);
            Baggage.Current = parentContext.Baggage;

            // Start an activity with a name following the semantic convention of the OpenTelemetry messaging specification.
            // https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/messaging.md#span-name
            var activityName = $"{meta.Operation} consume";

            using var activity = activitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
            return activity;
        }

        #endregion // StartSpanScope
    }
}
