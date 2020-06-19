using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Event Source producer builder.
    /// </summary>
    public interface IEventSourceProducerCustomBuilder
        : IEventSourceProducer2Builder
    {
        /// <summary>
        /// Custom source.
        /// </summary>
        /// <param name="customSource">The custom source should be used carefully,
        /// and only when the data shouldn't be sequence with other sources.</param>
        /// <returns></returns>
        IEventSourceProducer2Builder CustomSource(
                                    string customSource);
    }
}
