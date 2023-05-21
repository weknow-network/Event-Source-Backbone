using static System.StringComparison;

namespace EventSourcing.Backbone
{
    /// <summary>
    /// Common Constants
    /// </summary>
    public class Env
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

        #endregion // Cast overloads

        #region Format

        /// <summary>
        /// Formats the specified environment into WeKnow convention.
        /// </summary>
        /// <returns></returns>
        public string Format() => Env.Format(_value);

        /// <summary>
        /// Formats the specified environment into WeKnow convention.
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
    }
}
