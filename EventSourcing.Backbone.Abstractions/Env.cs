using System.Text.Json;
using System.Text.Json.Serialization;

using static System.StringComparison;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Common Constants
    /// </summary>
    [JsonConverter(typeof(EnvJsonConverter))]
    public sealed class Env : IEquatable<Env?>
    {
        private readonly string _value;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value">The value.</param>
        public Env(string value)
        {
            _value = Env.Format(value);
        }

        #endregion // Ctor

        #region Cast overloads

        /// <summary>
        /// Performs an implicit conversion from <see cref="string"/> to <see cref="Env"/>.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Env(string env) => new Env(env);

        /// <summary>
        /// Performs an implicit conversion from <see cref="Env"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="env">The env.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(Env env) => env._value;

        public static bool operator ==(Env? left, Env? right)
        {
            return EqualityComparer<Env>.Default.Equals(left, right);
        }

        public static bool operator !=(Env? left, Env? right)
        {
            return !(left == right);
        }

        #endregion // Cast overloads

        #region Format

        /// <summary>
        /// Formats the specified environment into  convention.
        /// </summary>
        /// <returns></returns>
        public string Format() => Env.Format(_value);

        /// <summary>
        /// Formats the specified environment into  convention.
        /// </summary>
        /// <returns></returns>
        private static string Format(string e)
        {
            bool comp(string candidate) => string.Compare(e, candidate, OrdinalIgnoreCase) == 0;

            if (comp("Production")) return "prod";
            if (comp("Prod")) return "prod";
            if (comp("Development")) return "dev";
            if (comp("Dev")) return "dev";

            return e;
        }

        #endregion // Format

        #region ToString

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString() => _value;

        #endregion // ToString

        #region EnvJsonConverter

        /// <summary>
        /// Env Json Converter
        /// </summary>
        private sealed class EnvJsonConverter : JsonConverter<Env>
        {
            public override Env Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options) =>
                    new Env(reader.GetString()!);

            public override void Write(
                Utf8JsonWriter writer,
                Env env,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue(env.ToString());
        }

        #endregion // EnvJsonConverter

        #region IEquatable<Env?> members

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as Env);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(Env? other)
        {
            return other is not null &&
                   _value == other._value;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        #endregion // IEquatable<Env?> members
    }
}
