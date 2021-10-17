using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using static Weknow.Text.Json.Constants;

namespace Weknow.EventSource.Backbone
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
        /// A <see cref="System.String" /> that represents this instance.
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

        ///// <summary>
        ///// Performs an implicit conversion.
        ///// </summary>
        ///// <param name="ids">The identifiers.</param>
        ///// <returns>
        ///// The result of the conversion.
        ///// </returns>
        //public static implicit operator EventKeys(JsonElement[] ids) => new EventKeys(ids);

        /// <summary>
        /// Performs an implicit conversion from <see cref="EventKeys"/> to <see cref="JsonElement"/>.
        /// </summary>
        /// <param name="ids">The identifiers.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JsonElement[](EventKeys ids) => ids.Select(m => (JsonElement)m).ToArray();


        #endregion // Cast overloads
    }
}
