namespace BuildABot.Core.Feedback
{
    /// <summary>
    /// Type of feedback provided by the bot client.
    /// </summary>
    public enum FeedbackType
    {
        /// <summary>
        /// No feedback provided by the user.
        /// </summary>
        NotProvided,

        /// <summary>
        /// Positive feedback.
        /// </summary>
        Positive,

        /// <summary>
        /// Negative feedback.
        /// </summary>
        Negative,
    }
}
