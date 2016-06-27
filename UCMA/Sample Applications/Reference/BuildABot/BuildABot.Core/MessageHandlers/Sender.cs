namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// The sender kind.
    /// </summary>
    public enum SenderKind
    {
        /// <summary>
        /// The sender kind is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The sender is an end-user.
        /// </summary>
        User,

        /// <summary>
        /// The sender is a system or application.
        /// </summary>
        System,
    }

    /// <summary>
    /// Message sender.
    /// </summary>
    public class Sender
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sender"/> class.
        /// </summary>
        public Sender()
        {
            this.DisplayName = string.Empty;
            this.Alias = string.Empty;
            this.Kind = SenderKind.Unknown;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sender"/> class.
        /// </summary>
        /// <param name="displayName">The display name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="kind">The kind.</param>
        public Sender(string displayName, string alias, SenderKind kind)
        {
            this.DisplayName = displayName;
            this.Alias = alias;
            this.Kind = kind;
        }


        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The display name.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the alias.
        /// </summary>
        /// <value>
        /// The alias.
        /// </value>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the kind.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        public SenderKind Kind { get; set; }


    }
}
