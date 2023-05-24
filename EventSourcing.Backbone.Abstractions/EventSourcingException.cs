using System.Runtime.Serialization;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event sourcing exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class EventSourcingException : Exception
    {
        public EventSourcingException() : base()
        {
        }

        public EventSourcingException(string? message) : base(message)
        {
        }

        public EventSourcingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EventSourcingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
