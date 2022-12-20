using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Weknow.EventSource.Backbone
{
    public static class TelemetryrExtensions
    {
        private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

        #region ExtractSpan

        /// <summary>
        /// Extract telemetry span's parent info
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meta">The meta.</param>
        /// <param name="entries">The entries for extraction.</param>
        /// <param name="injectStrategy">The injection strategy.</param>
        /// <returns></returns>
        public static ActivityContext ExtractSpan<T>(
                        this Metadata meta,
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
    }
}
