namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Message handler with only one single state.
    /// </summary>
    public abstract class SingleStateMessageHandler : MessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStateMessageHandler"/> class.
        /// </summary>
        public SingleStateMessageHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStateMessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        public SingleStateMessageHandler(string regexPattern) : base(regexPattern)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStateMessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        public SingleStateMessageHandler(string regexPattern, string initialHandlingText)
            : base(regexPattern, initialHandlingText)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStateMessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        /// <param name="requiresFeedback">if set to <c>true</c> [requires feedback].</param>
        public SingleStateMessageHandler(string regexPattern, string initialHandlingText, bool requiresFeedback)
            : base(regexPattern, initialHandlingText, requiresFeedback)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStateMessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        /// <param name="requiresFeedback">if set to <c>true</c> [requires feedback].</param>
        /// <param name="abortOnNeverMind">if set to <c>true</c> [abort on never mind].</param>
        public SingleStateMessageHandler(string regexPattern, string initialHandlingText, bool requiresFeedback, bool abortOnNeverMind)
            : base(regexPattern, initialHandlingText, requiresFeedback, abortOnNeverMind)
        {
        }

        /// <summary>
        /// Gets the start state handler.
        /// </summary>
        /// <value>The start state handler.</value>
        protected override StateHandler InitialStateHandler
        {
            get { return SingleStateHandle; }
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        private Reply SingleStateHandle(Message message)
        {
            Reply reply = Handle(message);
            Done = true;
            return reply;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public new abstract Reply Handle(Message message);
    }
}
