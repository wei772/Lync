using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Response of a message handler.
    /// </summary>
    public class MessageHandlingResponse
    {       
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlingResponse"/> class.
        /// </summary>
        public MessageHandlingResponse()
        {
        }

        /// <summary>
        /// A value from 0 to 1 that tells how confident the message handler can handle the message.
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// The text to be displayed before the request is handled.
        /// </summary>
        public string InitialHandlingText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlingResponse"/> class.
        /// </summary>
        /// <param name="confidence">The confidence.</param>
        /// <param name="initialHandlingText">The initial handling text.</param>
        public MessageHandlingResponse(double confidence, string initialHandlingText)
        {
            this.Confidence = confidence;
            this.InitialHandlingText = initialHandlingText;
        }
    }
}
