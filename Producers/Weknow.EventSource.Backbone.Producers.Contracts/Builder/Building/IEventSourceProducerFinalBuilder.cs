using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone.Building
{

    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    /// <typeparam name="T">The type of the sending data</typeparam>
    public interface IEventSourceProducerFinalBuilder<T>
        where T : class
    {
        /// <summary>
        /// Builds producer instance.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}
