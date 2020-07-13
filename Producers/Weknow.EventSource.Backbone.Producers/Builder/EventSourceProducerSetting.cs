using System.Collections.Immutable;

using Weknow.EventSource.Backbone.Building;

// TODO: Specify channel & tags for current writer
// TODO: Specify hierarchy?

namespace Weknow.EventSource.Backbone.Settings
{
    /// <summary>
    /// Event Source producer setting (used by the channel provider).
    /// </summary>
    public class EventSourceProducerSetting
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EventSourceProducerSetting"/> class.
        /// </summary>
        internal EventSourceProducerSetting() { }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="copyFrom">The copy from.</param>
        /// <param name="options">The options.</param>
        /// <param name="rawInterceptors">The raw interceptors.</param>
        internal EventSourceProducerSetting(
                        EventSourceProducerSetting copyFrom,
                        EventSourceOptions? options = null,
                        IImmutableQueue<IProducerRawAsyncInterceptor>? rawInterceptors = null)
        {
            Options = options ?? copyFrom.Options;
            RawInterceptors = rawInterceptors ?? copyFrom.RawInterceptors;
        }

        #endregion // Ctor

        #region Options

        /// <summary>
        /// Gets the options.
        /// </summary>
        public EventSourceOptions Options { get; } = EventSourceOptions.Empty;

        #endregion // Options

        #region RawInterceptors

        /// <summary>
        /// Gets the raw interceptors.
        /// </summary>
        public IImmutableQueue<IProducerRawAsyncInterceptor> RawInterceptors { get; } =
                                        ImmutableQueue<IProducerRawAsyncInterceptor>.Empty;

        #endregion // RawInterceptors
    }

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public class EventSourceProducerSetting<T>: EventSourceProducerSetting
        where T:notnull
    {
        internal EventSourceProducerSetting(EventSourceProducerSetting origin)
                    :base(origin)
        {
        }
        internal EventSourceProducerSetting(
                        EventSourceProducerSetting<T> copyFrom,
                        ): base(copyFrom)
        {
        }


        private readonly string _defaultEventName;
        private readonly IImmutableQueue<IProducerAsyncDecorator<T>> _typedInterceptor =
                                        ImmutableQueue<IProducerAsyncDecorator<T>>.Empty;
        private readonly IImmutableQueue<IProducerSegmenationProvider<T>> _segmentations =
                                        ImmutableQueue<IProducerSegmenationProvider<T>>.Empty;

    }
}
