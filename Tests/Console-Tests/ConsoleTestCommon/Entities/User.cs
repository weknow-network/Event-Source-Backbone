namespace Weknow.EventSource.Backbone.ConsoleTests
{
    public class User
    {
        public User(){}

        public User(string name, string id)
        {
            Eracure = new Personal { GovernmentId = id, Name = name };
        }

        /// <summary>
        /// GDPR the right to be forgotten
        /// </summary>
        public Personal Eracure { get; set; }
        /// <summary>
        /// Non-personal data
        /// </summary>
        public Anonymous Details { get; set; }

        /// <summary>
        /// GDPR the right to be forgotten
        /// </summary>
        public class Personal
        {
            public string Name { get; set; } = string.Empty;
            public string GovernmentId { get; set; } = string.Empty;
        }

        /// <summary>
        /// Non-personal data
        /// </summary>
        public class Anonymous
        {
            public int Id { get; set; }
            public string Hobbies { get; set; } = string.Empty;
        }

        public override string ToString() => $"{Eracure?.Name}";
    }
}
