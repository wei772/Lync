using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Util;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Defines methods that handle states.
    /// </summary>
    public delegate Reply StateHandler(Message message);

    /// <summary>
    /// Handle user messages.
    /// </summary>
    public abstract class MessageHandler    
    {
        /// <summary>
        /// This message handler's input matcher.
        /// </summary>
        protected InputMatcher inputMatcher;

        /// <summary>
        /// The next state handler (method). 
        /// </summary>
        protected StateHandler nextStateHandler;

        private const string abortResponseText = "alright";
        private static string[] abortRequestTexts = { "never mind", "cancel", "stop", "done", "abort" };

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        public MessageHandler()
        {
            this.nextStateHandler = this.InitialStateHandler;
            this.RequiresFeedback = false;
            this.AbortOnNeverMind = true;
            this.DefaultConfidence = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        public MessageHandler(string regexPattern)
        {
            this.nextStateHandler = this.InitialStateHandler;
            this.RequiresFeedback = false;
            this.AbortOnNeverMind = true;
            this.DefaultConfidence = 1;
            this.inputMatcher = new InputMatcher(regexPattern);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        public MessageHandler(string regexPattern, string initialHandlingText)
            : this(regexPattern)
        {
            this.InitialHandlingText = initialHandlingText;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        /// <param name="requiresFeedback">if set to <c>true</c> [requires feedback].</param>
        public MessageHandler(string regexPattern, string initialHandlingText, bool requiresFeedback)
            : this(regexPattern, initialHandlingText)
        {
            this.RequiresFeedback = requiresFeedback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandler"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        /// <param name="requiresFeedback">if set to <c>true</c> [requires feedback].</param>
        /// <param name="abortOnNeverMind">if set to <c>true</c> [abort on never mind].</param>
        public MessageHandler(string regexPattern, string initialHandlingText, bool requiresFeedback, bool abortOnNeverMind)
            : this(regexPattern, initialHandlingText, requiresFeedback)
        {
            this.AbortOnNeverMind = abortOnNeverMind;
        }


        /// <summary>
        /// Gets or sets the default confidence.
        /// </summary>
        /// <value>The default confidence.</value>
        public virtual double DefaultConfidence { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this message handler requires feedback from the user.
        /// </summary>
        /// <value><c>true</c> if requires feedback; otherwise, <c>false</c>.</value>
        public bool RequiresFeedback { get; set; }


        /// <summary>
        /// Gets the initial state handler.
        /// </summary>
        /// <value>The initial state handler.</value>
        protected abstract StateHandler InitialStateHandler { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this state machine handler should abort hanlding messages if the
        /// user says "never mind" or similar.
        /// </summary>
        /// <value><c>true</c> if [abort on never mind]; otherwise, <c>false</c>.</value>
        protected bool AbortOnNeverMind { get;  set; }

        /// <summary>
        /// Gets or sets the initial handling text.
        /// </summary>
        /// <value>The initial handling text.</value>
        protected string InitialHandlingText { get; set; }

        /// <summary>
        /// Gets the parameter value given the specified parameter name.
        /// </summary>
        /// <value></value>
        protected virtual string this[string parameterName]
        {
            get
            {
                string result = null;
                if (this.inputMatcher != null)
                {
                    return this.inputMatcher[parameterName];
                }
                return result;
            }
        }



        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message info.</param>
        /// <returns></returns>
        public virtual MessageHandlingResponse CanHandle(Message message)
        {
            MessageHandlingResponse response = new MessageHandlingResponse();
            if (this.inputMatcher != null && inputMatcher.CanHandle(message.Content))
            {
                response.Confidence = this.DefaultConfidence;
                response.InitialHandlingText = this.GetInitialHandlingText();
            }

            return response;
        }

        /// <summary>
        /// Gets the initial handling text. By this time, you can already call the inputMatcher index to request regex group values.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetInitialHandlingText()
        {
            return this.InitialHandlingText;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MessageHandler"/> is done.
        /// </summary>
        /// <value><c>true</c> if done; otherwise, <c>false</c>.</value>
        public bool Done { get; set; }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Reply Handle(Message message)
        {
            Reply reply;
            if (this.Done)
            {
                // Programming error: can't handle a message if this message handler is already done.
                throw new DoneMessageHandlerStateCalledException();
            }
            else if (this.AbortOnNeverMind && abortRequestTexts.Contains(message.Content.ToLower()))
            {
                reply = new Reply(abortResponseText);
                this.Done = true;
            }
            else
            {
                reply = this.nextStateHandler(message);
            }

            return reply;
        }
    }
}
