using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.Json;

using static EventSourcing.Backbone.EventSourceConstants;

namespace EventSourcing.Backbone
{
    public class Bucket : IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>
    {
        /// <summary>
        /// Empty
        /// </summary>
        public static readonly Bucket Empty = new Bucket();
        private readonly ImmutableDictionary<string, ReadOnlyMemory<byte>> _data =
                            ImmutableDictionary<string, ReadOnlyMemory<byte>>.Empty;

        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="Bucket"/> class from being created.
        /// </summary>
        private Bucket()
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="data">The data.</param>
        private Bucket(ImmutableDictionary<string, ReadOnlyMemory<byte>> data)
        {
            _data = data;
        }

        #endregion // Ctor

        #region Add

        /// <summary>
        /// Adds an element with the specified key and value to the bucket.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public Bucket Add(string key, ReadOnlyMemory<byte> value)
        {
            return _data.Add(key, value);
        }

        #endregion // Add

        #region AddRange

        /// <summary>
        /// Adds an elements with the specified key and value to the bucket.
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public Bucket AddRange(IEnumerable<(string key, byte[] value)> pairs)
        {
            var item = pairs.Select(m => new KeyValuePair<string, ReadOnlyMemory<byte>>(m.key, m.value));
            return _data.AddRange(item);
        }

        /// <summary>
        /// Adds the specified key/value pairs to the bucket.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <returns></returns>
        public Bucket AddRange(Bucket bucket)
        {
            return _data.AddRange(bucket);
        }

        #endregion // AddRange

        #region TryAddRange

        /// <summary>
        /// Adds an elements with the specified key and value to the bucket if the key doesn't exists.
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public Bucket TryAddRange(IEnumerable<(string key, byte[] value)> pairs) => TryAddRange(pairs.Select(m => ((string key, byte[] value)?)m));

        /// <summary>
        /// Adds an elements with the specified key and value to the bucket if the key doesn't exists.
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public Bucket TryAddRange(IEnumerable<(string key, byte[] value)?> pairs)
        {
            var result = pairs.Aggregate(_data, (acc, pair) =>
            {
#pragma warning disable CS8604 // Possible null reference argument.
                if (!pair.HasValue || pair?.value == null || acc.ContainsKey(pair?.key))
                    return acc;
                return acc.Add(pair?.key, pair?.value);
#pragma warning restore CS8604 // Possible null reference argument.
            });
            return result;
        }

        /// <summary>
        /// Adds the specified key/value pairs to the bucket if the key doesn't exists.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        /// <returns></returns>
        public Bucket TryAddRange(Bucket bucket)
        {
            var result = bucket.Aggregate(_data, (acc, pair) =>
            {
                var (key, value) = pair;
                if (acc.ContainsKey(key))
                    return acc;
                return acc.Add(key, value);
            });
            return result;
        }

        #endregion // TryAddRange

        #region RemoveRange

        /// <summary>
        /// Removes keys from the bucket.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public Bucket RemoveRange(params string[] keys) => RemoveRange((IEnumerable<string>)keys);

        /// <summary>
        /// Removes keys from the bucket.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public Bucket RemoveRange(IEnumerable<string> keys)
        {
            return _data.RemoveRange(keys);
        }

        #endregion // RemoveRange

        #region Remove

        /// <summary>
        /// Removes items from the bucket.
        /// </summary>
        /// <param name="filter">The filter by key.</param>
        /// <returns></returns>
        public Bucket RemoveRange(Predicate<string>? filter = null)
        {
            if (filter == null) return this;

            IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>> filtered = _data.Where(m => filter(m.Key));
            return ImmutableDictionary.CreateRange<string, ReadOnlyMemory<byte>>(filtered);
        }

        #endregion // RemoveRange

        #region ContainsKey

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(string key)
        {
            return _data.ContainsKey(key);
        }

        #endregion // ContainsKey

        #region TryGetValue

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out ReadOnlyMemory<byte> value)
        {
            if (_data.TryGetValue(key, out value))
                return true;
            if (_data.TryGetValue(key.ToPascal(), out value))
                return true;
            if (_data.TryGetValue(key.ToCamel(), out value))
                return true;
            if (_data.TryGetValue(key.ToDash(), out value))
                return true;
            return _data.TryGetValue(key.ToLower(), out value);
        }

        #endregion // TryGetValue

        #region TryGet

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T? value)
        {
            value = default;
            if (_data.TryGetValue(key, out ReadOnlyMemory<byte> v))
            {
                var result = JsonSerializer.Deserialize<T>(v.Span, SerializerOptionsWithIndent);
                value = result;
                return value != null;
            }
            return false;
        }

        #endregion // TryGet

        #region Keys

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public IEnumerable<string> Keys => _data.Keys;

        #endregion // Keys

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Bucket(ImmutableDictionary<string, ReadOnlyMemory<byte>> data)
        {
            return new Bucket(data);
        }

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ImmutableDictionary<string, ReadOnlyMemory<byte>>(Bucket instance)
        {
            return instance._data;
        }

        /// <summary>
        /// Performs an implicit conversion.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ImmutableDictionary<string, string>(Bucket instance)
        {
            var result = instance.Aggregate(ImmutableDictionary<string, string>.Empty,
                (acc, m) =>
            {
                byte[] bytes = m.Value.ToArray();
                var json = Encoding.UTF8.GetString(bytes);
                return acc.Add(m.Key, json);

            });
            return result;
        }

        #endregion // Cast overloads

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, ReadOnlyMemory<byte>>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion // IEnumerable
    }
}