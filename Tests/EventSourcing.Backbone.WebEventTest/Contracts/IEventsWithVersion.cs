using System.ComponentModel;

namespace EventSourcing.Backbone.WebEventTest
{
    [EventsContract(EventsContractType.Consumer, MinVersion = 1, IgnoreVersion = new [] { 3 }, VersionNaming = VersionNaming.None)]
    [EventsContract(EventsContractType.Producer, MinVersion = 2)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Used for code generation, use the producer / consumer version of it", true)]
    public interface IEventsWithVersion
    {
        /// <summary>
        /// Logins the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        [EventSourceVersion(1, Retired = 3)] // same as not specifying a the attribute at all.
        ValueTask Login(string name, string password);

        /// <summary>
        /// Stages version 0.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [EventSourceVersion(0, Retired = 3)] // same as not specifying a the attribute at all.
        ValueTask Stage(string name);

        /// <summary>
        /// Stages version 1.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(3, Retired = 4)] // same as not specifying a the attribute at all.
        ValueTask Stage(int value);

        /// <summary>
        /// Stages version 6.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(4)]
        ValueTask Stage(string name, byte value);

        /// <summary>
        /// Pings the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(2, Retired = 5)]
        ValueTask Ping(int value);

        /// <summary>
        /// Pings the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [EventSourceVersion(5)]
        ValueTask Ping(string value);
    }
}
