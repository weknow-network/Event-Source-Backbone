using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder stage.
    /// </summary>
    public interface IConsumerCancellationBuilder<T>
    {

        #region WithCancellation

        /// <summary>
        /// Withes the cancellation token.
        /// </summary>
        /// <param name="cancellation">The cancellation.</param>
        /// <returns></returns>
        T WithCancellation(CancellationToken cancellation);

        #endregion // WithCancellation
    }
}
