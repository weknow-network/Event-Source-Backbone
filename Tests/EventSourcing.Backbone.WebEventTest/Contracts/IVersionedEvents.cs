using System.ComponentModel;

namespace EventSourcing.Backbone.WebEventTest
{
    [EventsContract(EventsContractType.Consumer)]
    [EventsContract(EventsContractType.Producer)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Used for code generation, use the producer / consumer version of it", true)]
    public interface IVersionedEvents
    {
        /// <summary>
        /// Stages version 1.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        // [EventSourceVersion(0)] // same as not specifying a the attribute at all.
        ValueTask Stage(string name);

        /// <summary>
        /// Stages version 3.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(4, ConsumeFrom = 1)]
        ValueTask Stage1(int value);

        /// <summary>
        /// Stages version 6.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(6, ConsumeFrom = 5)]
        ValueTask Stage2(string name, byte value);
    }
}
