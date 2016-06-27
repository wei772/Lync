using System;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Exception that should be raised when a done message handler is invoked.
    /// </summary>
    internal class DoneMessageHandlerStateCalledException : Exception
    {
        private const string doneMessageHandlerCalledErrorMessage = "Done state machine message handlers should not be invoked.";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DoneMessageHandlerStateCalledException"/> class.
        /// </summary>
        public DoneMessageHandlerStateCalledException() : base(doneMessageHandlerCalledErrorMessage) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoneMessageHandlerStateCalledException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DoneMessageHandlerStateCalledException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoneMessageHandlerStateCalledException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public DoneMessageHandlerStateCalledException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DoneMessageHandlerStateCalledException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected DoneMessageHandlerStateCalledException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
