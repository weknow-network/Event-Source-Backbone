using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Represent acknowledge trigger which will 
    /// set the status on the event as done and release 
    /// it for re-consuming.
    /// </summary>
    public interface IAck
    {
        /// <summary>
        /// Acknowledge the asynchronous.
        /// </summary>
        /// <returns></returns>
        ValueTask AckAsync();
        /// <summary>
        /// Revokes the operation if not completed after a duration
        /// and avoid re-consume of the message (by same consumer signature).
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        void AckAfter(TimeSpan timeout);
    }
}
