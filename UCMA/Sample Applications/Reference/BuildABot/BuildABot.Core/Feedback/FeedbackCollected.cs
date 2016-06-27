namespace BuildABot.Core.Feedback
{
    using System;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// FeedbackEventHandler delegate.
    /// </summary>
    public delegate Reply FeedbackEventHandler(object sender, FeedbackCollectedEventArgs e);
    
    /// <summary>
    /// Event arguments for the FeedbackCollected delegate.
    /// </summary>
    public class FeedbackCollectedEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackCollectedEventArgs"/> class.
        /// </summary>
        /// <param name="feedbackType">Type of the feedback.</param>
        /// <param name="feedbackMessage">The feedback message.</param>
        /// <param name="originalMessage">The original message.</param>
        public FeedbackCollectedEventArgs(FeedbackType feedbackType, Message feedbackMessage, Message originalMessage)
        {
            this.FeedbackType = feedbackType;
            this.OriginalMessage = originalMessage;
            this.FeedbackMessage = feedbackMessage;
        }

        /// <summary>
        /// Gets or sets the type of the feedback.
        /// </summary>
        /// <value>The type of the feedback.</value>
        public FeedbackType FeedbackType { get; set; }

        /// <summary>
        /// Gets or sets the original message from the user. Not to be confused with the message 
        /// entered by the user to express feedback ("yes", "no", etc.).
        /// </summary>
        /// <value>The original message.</value>
        public Message OriginalMessage { get; set; }

        /// <summary>
        /// Gets or sets the feedback message. This is the message that the user entered when the bot requested feedback.
        /// </summary>
        /// <value>The feedback message.</value>
        public Message FeedbackMessage { get; set; }
    }
}
