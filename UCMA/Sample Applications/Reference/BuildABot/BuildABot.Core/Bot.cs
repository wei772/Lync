namespace BuildABot.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using BuildABot.Core.Feedback;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// Conversational agent.
    /// </summary>
    public class Bot
    {

        /// <summary>
        /// The messageHandlerCandidates for the current conversation.
        /// </summary>       
        private Dictionary<MessageHandler, MessageHandlingResponse> messageHandlerCandidates;

        /// <summary>
        /// Whether the bot is awaiting a new conversation.
        /// </summary>
        private bool isNewConversation = true;

        /// <summary>
        /// Whether the bot is in feedback collection mode.
        /// </summary>
        private bool isCollectingFeedback = false;


        /// <summary>
        /// The original (first) message of a conversation.
        /// </summary>
        private Message originalMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        public Bot()
        {
            FeedbackEngine = new FeedbackEngine();
        }


        /// <summary>
        /// Occurs  whenever the bot sends reply. 
        /// </summary>
        public event ReplyEventHandler Replied;

        /// <summary>
        /// Occurs whenever the bot receives a message.
        /// </summary>
        public event MessageEventHandler MessageReceived;

        /// <summary>
        /// Occurs whenever the bot fails to understand the user.
        /// </summary>
        public event MessageEventHandler FailedToUnderstand;

        /// <summary>
        /// Sets a value indicating whether bots should use emoticons.
        /// </summary>
        /// <value><c>true</c> if bots should use emoticons; otherwise, <c>false</c>.</value>
        public static bool UseEmoticons
        {
            set
            {
                Util.Emoticons.Enabled = value;
            }
        }

        /// <summary>
        /// The message handler that the bot is using to handle the current conversation.
        /// </summary>
        public MessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this bot should give up on trying to handle the user
        /// request after receiving negative feedback.
        /// </summary>
        /// <value>
        /// <c>true</c> if the bot should give up on trying to handle the user request after receiving negative feedback; otherwise, <c>false</c>.
        /// </value>
        public bool GiveUpOnNegativeFeedback { get; set; }

        /// <summary>
        /// Gets or sets this bot's feedback engine.
        /// </summary>
        /// <value>The feedback engine.</value>
        public FeedbackEngine FeedbackEngine { get; set; }

        /// <summary>
        /// Gets or sets the conversation reply count.
        /// </summary>
        /// <value>The conversation iteration.</value>
        public int ConversationReplyCount { get; set; }


        /// <summary>
        /// Determines whether this class can process the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the specified message; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanProcess(Message message)
        {
            return Bot.GetMessageHandlerCandidates(message).Count > 0;
        }

        /// <summary>
        /// Processes an incoming message, raising events accordingly.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void ProcessMessage(Message message)
        {
            this.ProcessMessage(message, false);
        }

        /// <summary>
        /// Processes an incoming message, raising events accordingly.
        /// </summary>
        /// <param name="messageContent">Content of the message.</param>
        public virtual void ProcessMessage(string messageContent)
        {
            this.ProcessMessage(new Message(messageContent), false);
        }

        /// <summary>
        /// Pre-processes the message content, trimming the string and removing extra spaces.
        /// </summary>
        /// <param name="messageContent">The message content.</param>
        /// <returns></returns>
        public virtual string PreProcessMessageContent(string messageContent)
        {
            string[] parts = messageContent.Trim().Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts);
        }

        /// <summary>
        /// Gets the best message handler from a list of candidates.
        /// </summary>
        /// <param name="messageHandlerCandidates">The message handler candidates.</param>
        /// <returns></returns>
        private static MessageHandler GetBestMessageHandler(Dictionary<MessageHandler, MessageHandlingResponse> messageHandlerCandidates)
        {
            MessageHandler bestMessageHandler = null;
            double greatestConfidence = double.MinValue;
            foreach (MessageHandler messageHandlerCandidate in messageHandlerCandidates.Keys)
            {
                if (messageHandlerCandidates[messageHandlerCandidate].Confidence > greatestConfidence)
                {
                    bestMessageHandler = messageHandlerCandidate;
                    greatestConfidence = messageHandlerCandidates[messageHandlerCandidate].Confidence;
                }
            }
            return bestMessageHandler;
        }

        /// <summary>
        /// Gets the message handler candidates for the specified message, i.e., the message handlers which return a confidence
        /// higher than one for the current message.
        /// </summary>
        /// <param name="message">The message info.</param>
        /// <returns></returns>
        private static Dictionary<MessageHandler, MessageHandlingResponse> GetMessageHandlerCandidates(Message message)
        {
            Dictionary<MessageHandler, MessageHandlingResponse> messageHandlerCandidates = new Dictionary<MessageHandler, MessageHandlingResponse>();
            List<MessageHandler> allMessageHandlers = MessageHandlerFactory.InitializeMessageHandlers();

            foreach (MessageHandler messageHandlerCandidate in allMessageHandlers)
            {
                MessageHandlingResponse handlingResponse = messageHandlerCandidate.CanHandle(message);
                if (handlingResponse.Confidence > 0)
                {
                    messageHandlerCandidates[messageHandlerCandidate] = handlingResponse;
                }
            }
            return messageHandlerCandidates;
        }

        /// <summary>
        /// Processes an incoming message, raising events accordingly. This is the most important method of all bot functionality.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="isReprocessing">if set to <c>true</c> [is reprocessing].</param>
        private void ProcessMessage(Message message, bool isReprocessing)
        {
            if (!isReprocessing)
            {
                Debug.WriteLine("Bot received: " + message.Content);
                if (this.MessageReceived != null)
                {
                    // raising message received event
                    this.MessageReceived(this, new MessageEventArgs(message));
                }
            }

            message.Content = this.PreProcessMessageContent(message.Content);

            if (this.isCollectingFeedback)
            {
                this.isCollectingFeedback = false;

                FeedbackType feedbackType;
                Reply feedbackReply = FeedbackEngine.ProcessFeedback(message, this.originalMessage, out feedbackType);
                this.RaiseReplied(feedbackReply, message, ReplyContext.FeedbackResponse, null);

                // If the user didn't provide feedback, we try handling the message again, which will
                // re-start the handling process as the bot is not in feedback collection mode anymore.
                if (feedbackType == FeedbackType.NotProvided)
                {
                    this.ProcessMessage(message, true);
                }

                else if (feedbackType == FeedbackType.Negative)
                {
                    if (!this.GiveUpOnNegativeFeedback)
                    {
                        // The current handler is no good anymore (negative feedback received).
                        this.messageHandlerCandidates.Remove(MessageHandler);

                        // Let's try the other ones!
                        this.ProcessNewMessage(this.originalMessage, this.messageHandlerCandidates);
                    }
                }
            }

            // is this message the start of a new conversation?
            else if (this.isNewConversation)
            {
                this.ConversationReplyCount = 0;
                this.originalMessage = message;
                this.messageHandlerCandidates = GetMessageHandlerCandidates(message);
                this.ProcessNewMessage(message, this.messageHandlerCandidates);
            }

            // else, this message is part of an already existent conversation.
            else
            {
                this.InvokeMessageHandler(MessageHandler, message);
            }
        }



        /// <summary>
        /// Processes a new message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageHandlerCandidates">The message handler candidates.</param>
        private void ProcessNewMessage(Message message, Dictionary<MessageHandler, MessageHandlingResponse> messageHandlerCandidates)
        {
            if (messageHandlerCandidates.Keys.Count > 0)
            {
                this.MessageHandler = GetBestMessageHandler(messageHandlerCandidates);

                if (!string.IsNullOrEmpty(messageHandlerCandidates[MessageHandler].InitialHandlingText))
                {
                    // send initial text in an async manner
                    Reply firstReply = new Reply(messageHandlerCandidates[MessageHandler].InitialHandlingText);
                    this.RaiseReplied(firstReply, message, ReplyContext.InitialHandlingText, MessageHandler);
                }

                this.InvokeMessageHandler(MessageHandler, message);
            }

            else if (message.Sender.Kind != SenderKind.System)
            {
                if (this.FailedToUnderstand != null)
                {
                    this.FailedToUnderstand(this, new MessageEventArgs(message));
                }
            }
        }


        /// <summary>
        /// Invokes a message handler.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageHandler">The message handler.</param>
        private void InvokeMessageHandler(MessageHandler messageHandler, Message message)
        {
            Reply reply = messageHandler.Handle(message);
            this.RaiseReplied(reply, message, ReplyContext.RegularReplyMessage, messageHandler);
            this.isNewConversation = messageHandler.Done;

            // Is any feedback handler enabled for this bot?
            if (messageHandler.Done && messageHandler.RequiresFeedback)
            {
                this.RaiseReplied(this.FeedbackEngine.FeedbackRequest, message, ReplyContext.FeedbackRequest, null);
                this.isCollectingFeedback = true;
            }
        }


        /// <summary>
        /// Raises the replied event.
        /// </summary>
        /// <param name="reply">The reply.</param>
        /// <param name="message">The message.</param>
        /// <param name="replyContext">The reply context.</param>
        /// <param name="messageHandler">The message handler.</param>
        private void RaiseReplied(Reply reply, Message message, ReplyContext replyContext, MessageHandler messageHandler)
        {
            if (this.Replied != null && reply != null && reply.Messages != null && reply.Messages.Count > 0)
            {
                this.ConversationReplyCount++;
                this.Replied(this, new ReplyEventArgs(reply, message, replyContext, this.ConversationReplyCount, messageHandler));
                Debug.WriteLine("Bot says: ");
                Debug.WriteLine(reply);
            }
        }





    }
}
