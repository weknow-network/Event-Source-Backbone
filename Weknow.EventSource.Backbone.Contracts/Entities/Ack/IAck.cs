using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent acknowledge trigger which will 
    /// set the status on the event as done or release 
    /// it for re-consuming.
    /// </summary>
    public interface IAck: IDataflowBlock
    {
        /// <summary>
        /// Revokes the operation if not completed after a duration.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void RevokeAfter(TimeSpan timeout);
    }
}
