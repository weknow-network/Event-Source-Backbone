namespace EventSourcing.Backbone.Channels
{
    /// <summary>
    /// Response structure
    /// </summary>
    public class BlobResponse : IEquatable<BlobResponse?>
    {
        #region Ctor

        /// <summary>
        /// Prevents a default instance of the <see cref="BlobResponse" /> class from being created.
        /// </summary>
        [Obsolete("Use other constructors (this one exists to enable de-serialization)", true)]
        private BlobResponse() { }

        /// <summary>
        /// Create request instance.
        /// </summary>
        /// <param name="key">The blob key.</param>
        /// <param name="eTag">The e tag.</param>
        /// <param name="contentVersion">The content version.</param>
        /// <param name="fileName">Name of the file.</param>
        /// 
        public BlobResponse(
            string key,
            string eTag,
            string contentVersion,
            string? fileName = null)
        {
            _key = key;
            _eTag = eTag;
            _contentVersion = contentVersion;
            _fileName = fileName;
        }

        #endregion // Ctor

        #region Key

        private string _key = string.Empty;
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key
        {
            get => _key;
            [Obsolete("Exposed for the serializer", true)]
            set => _key = value;
        }

        #endregion Key 

        #region FileName

        private string? _fileName = string.Empty;
        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string? FileName
        {
            get => _fileName;
            [Obsolete("Exposed for the serializer", true)]
            set => _fileName = value;
        }

        #endregion FileName 

        #region ETag

        private string _eTag = string.Empty;
        /// <summary>
        /// Gets or sets the eTag.
        /// </summary>
        public string ETag
        {
            get => _eTag;
            [Obsolete("Exposed for the serializer", true)]
            set => _eTag = value;
        }

        #endregion ETag 

        #region ContentVersion

        private string _contentVersion = string.Empty;
        /// <summary>
        /// Gets or sets the contentVersion.
        /// </summary>
        public string ContentVersion
        {
            get => _contentVersion;
            [Obsolete("Exposed for the serializer", true)]
            set => _contentVersion = value;
        }

        #endregion ContentVersion 

        #region Equality

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as BlobResponse);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(BlobResponse? other)
        {
            return other != null &&
                   _key == other._key &&
                   _fileName == other._fileName &&
                   _eTag == other._eTag &&
                   _contentVersion == other._contentVersion;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(
                _key,
                _fileName,
                _eTag,
                _contentVersion);
        }


        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(BlobResponse? left, BlobResponse? right)
        {
            return EqualityComparer<BlobResponse>.Default.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(BlobResponse? left, BlobResponse? right)
        {
            return !(left == right);
        }

        #endregion // Equality
    }
}
