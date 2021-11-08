using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;


namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Sampler which exclude path like health and readiness
    /// </summary>
    /// <seealso cref="OpenTelemetry.Trace.Sampler" />
    internal class TestSampler : Sampler
    {
        private Sampler? _sampler;
        private readonly IImmutableSet<string> _ignore = ImmutableHashSet.Create("XPENDING", "XREADGROUP", "XADD", "XACK");

        public static Sampler Create(LogLevel logLevel = LogLevel.Information, Sampler? chainedSampler = null)
            => new TestSampler(logLevel, chainedSampler);
        private readonly LogLevel _logLevel;

        #region Ctor

        private TestSampler(LogLevel logLevel = LogLevel.Information, Sampler? chainedSampler = null)
        {
            _logLevel = logLevel;
            _sampler = chainedSampler;
        }

        #endregion Ctor

        #region ShouldSample

        /// <summary>
        /// Checks whether span needs to be created and tracked.
        /// </summary>
        /// <param name="samplingParameters">The <see cref="T:OpenTelemetry.Trace.SamplingParameters" /> used by the <see cref="T:OpenTelemetry.Trace.Sampler" />
        /// to decide if the <see cref="T:System.Diagnostics.Activity" /> to be created is going to be sampled or not.</param>
        /// <returns>
        /// Sampling decision on whether Span needs to be sampled or not.
        /// </returns>
        public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
        {
            // Parent span context. Typically taken from the wire.
            ActivityContext parentContext = samplingParameters.ParentContext;
            // Trace ID of a span to be created.
            ActivityTraceId traceId = samplingParameters.TraceId;
            string name = samplingParameters.Name;

            if (_logLevel >= LogLevel.Information && _ignore.Contains(name))
                return new SamplingResult(false);
            //ActivityKind spanKind = samplingParameters.Kind;
            // Initial set of Attributes for the Span being constructed.
            //var attributes = samplingParameters.Tags;
            // Links associated with the span.
            //IEnumerable<ActivityLink> links = samplingParameters.Links;

            //var path = HttpRequestContext.Value?.Path.Value ?? "";
            //if (path == "/health" ||
            //    path == "/readiness" ||
            //    path == "/version" ||
            //    path == "/settings" ||
            //    path.StartsWith("/v1/kv/") || // configuration 
            //    path == "/api/v2/write" || // influx metrics
            //    path == "/_bulk" ||
            //    path.StartsWith("/swagger") ||
            //    path.IndexOf("health-check") != -1)
            //{
            //    return new SamplingResult(false);
            //}
            SamplingResult decision = _sampler?.ShouldSample(samplingParameters) ?? new SamplingResult(true);
            return decision;
        }

        #endregion ShouldSample
    }
}
