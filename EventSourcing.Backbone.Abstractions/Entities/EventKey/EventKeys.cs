using System.Collections;
using System.Collections.Immutable;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Event Id
    /// </summary>
    public class EventKeys : IEnumerable<EventKey>
    {
        private readonly ImmutableArray<EventKey> _keys = ImmutableArray<EventKey>.Empty;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public EventKeys(params EventKey[] keys)
        {
            _keys = _keys.AddRange(keys);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public EventKeys(IEnumerable<EventKey> keys)
        {
            _keys = _keys.AddRange(keys);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public EventKeys(params string[] keys)
        {
            _keys = _keys.AddRange(keys.Select(m => (EventKey)m));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public EventKeys(IEnumerable<string> keys)
        {
            _keys = _keys.AddRange(keys.Select(m => (EventKey)m));
        }

        ///// <summary>
        ///// Initializes a new instance.
        ///// </summary>
        ///// <param name="keys">The keys.</param>
        //public EventKeys(params JsonElement[] keys)
        //{
        //    _keys = _keys.AddRange(keys.Select(m => (EventKey)m));
        //}

        ///// <summary>
        ///// Initializes a new instance.
        ///// </summary>
        ///// <param name="keys">The keys.</param>
        //public EventKeys(IEnumerable<JsonElement> keys)
        //{
        //    _keys = _keys.AddRange(keys.Select(m => (EventKey)m));
        //}

        #endregion // Ctor

        #region EventKey this[int i]

        /// <summary>
        /// Gets the <see cref="EventKey"/> with the specified i.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public EventKey this[int i] => _keys[i];

        #endregion // EventKey this[int i]

        #region IEnumerable<EventKey> members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerator<EventKey> GetEnumerator()
        {
            foreach (var k in _keys)
            {
                yield return k;
            }
        }

        #endregion // IEnumerable<EventKey> members

        #region ToString

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Join(", ", _keys);

        #endregion // ToString

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator EventKeys(string[] ids) => new EventKeys(ids);

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator EventKeys(EventKey[] ids) => new EventKeys(ids);

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator EventKeys(ImmutableArray<EventKey> ids) => new EventKeys(ids);

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ImmutableArray<EventKey>(EventKeys ids) => ids._keys;

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string[](EventKeys ids) => ids.Select(m => (string)m).ToArray();

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(EventKeys ids) => ids.ToString();

        /// <summary>
        /// Performs an implicit conversion.
        /// Expecting single result
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static implicit operator EventKey(EventKeys ids) => ids.Single();

        ///// <summary>
        ///// Performs an implicit conversion.
        ///// </summary>
        ///// <param name="ids">The identifiers.</param>
        ///// <returns>
        ///// The result of the conversion.
        ///// </returns>
        //public static implicit operator EventKeys(JsonElement[] ids) => new EventKeys(ids);

        ///// <summary>
        ///// Performs an implicit conversion from <see cref="EventKeys"/> to <see cref="JsonElement"/>.
        ///// </summary>
        ///// <param name="ids">The identifiers.</param>
        ///// <returns>
        ///// The result of the conversion.
        ///// </returns>
        //public static implicit operator JsonElement[](EventKeys ids) => ids.Select(m => (JsonElement)m).ToArray();


        #endregion // Cast overloads
    }
}
