namespace BuildABot.Core.Feedback
{
    using System.Text.RegularExpressions;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// The feedback engine used for collecting the bot's client feedback.
    /// </summary>
    public class FeedbackEngine
    {
        private Reply feedbackRequest = new Reply("Did I understand you correctly?");

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackEngine"/> class.
        /// </summary>
        public FeedbackEngine()
        {
            this.PositiveFeedbackPattern = @"\byes\b|\byep\b|\by\b|\bsure\b";
            this.NegativeFeedbackPattern = @"\bno\b|\bnope\b|\bnot at all\b|\bn\b";
        }


        /// <summary>
        /// Occurs when feedback is collected.
        /// </summary>
        public event FeedbackEventHandler FeedbackCollected;

        /// <summary>
        /// Gets or sets the positive feedback pattern.
        /// </summary>
        /// <value>The positive feedback pattern.</value>
        public string PositiveFeedbackPattern { get; set; }

        /// <summary>
        /// Gets or sets the negative feedback pattern.
        /// </summary>
        /// <value>The negative feedback pattern.</value>
        public string NegativeFeedbackPattern { get; set; }

        /// <summary>
        /// Gets the feedback request.
        /// </summary>
        /// <value>The feedback request.</value>
        public virtual Reply FeedbackRequest
        {
            get
            {
                return this.feedbackRequest;
            }
            set
            {
                this.feedbackRequest = value;
            }
        }

        /// <summary>
        /// Processes the feedback.
        /// </summary>
        /// <param name="feedbackMessage">The feedback message.</param>
        /// <param name="originalMessage">The original message.</param>
        /// <param name="feedbackType">Type of the feedback.</param>
        /// <returns>
        /// The feedback type corresponding the the message.
        /// </returns>
        internal Reply ProcessFeedback(Message feedbackMessage, Message originalMessage, out FeedbackType feedbackType)
        {
            Reply reply = new Reply();
            feedbackType = FeedbackType.NotProvided;
            string feedback = feedbackMessage.Content.ToLower();
            if (Regex.IsMatch(feedback, this.PositiveFeedbackPattern))
            {
                feedbackType = FeedbackType.Positive;
            }
            else if (Regex.IsMatch(feedback, this.NegativeFeedbackPattern))
            {
                feedbackType = FeedbackType.Negative;
            }

            if (FeedbackCollected != null)
            {
                reply = FeedbackCollected(this, new FeedbackCollectedEventArgs(feedbackType, feedbackMessage, originalMessage));
            }

            return reply;
        }

    }
}
