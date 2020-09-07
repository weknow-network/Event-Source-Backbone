﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Weknow.EventSource.Backbone
{
    public class Bucket: IEnumerable<KeyValuePair<string, ReadOnlyMemory<byte>>>
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

        #region TryGetValue

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(string key, out ReadOnlyMemory<byte> value)
        {
            return _data.TryGetValue(key, out value);
        }

        #endregion // TryGetValue

        #region AddRange

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