using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuildABot.Core.MessageHandlers;
using Microsoft.Rtc.Collaboration;

namespace BuildABot.UC
{
    /// <summary>
    /// This class contains the properties related to conversation. 
    /// </summary>
    internal class ConversationProperties
    {
        /// <summary>
        /// Initializes an instance of ConversationProperties class
        /// </summary>
        /// <param name="destinationUri">sip uri of destination.</param>
        /// <param name="message">message sent by destination contact.</param>
        /// <param name="outgoingMessage">reply to be sent to the destination uri.</param>
        /// <param name="instantMessagingCall">An instance of instant messagin call.</param>
        public ConversationProperties(string destinationUri, Message message, Reply outgoingMessage, InstantMessagingCall instantMessagingCall)
        {
            this.DestinationUri = destinationUri;
            this.Message = message;
            this.OutgoingMessage = outgoingMessage;
            this.InstantMessagingCall = instantMessagingCall;
        }
        /// <summary>
        /// Gets or sets sip uri of destination.
        /// </summary>
        public string DestinationUri { get; set; }

        /// <summary>
        /// Gets or sets message sent by destination contact.
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Gets or sets reply to be sent to the destination uri.
        /// </summary>
        public Reply OutgoingMessage { get; set; }

        /// <summary>
        /// Gets or sets an instance of instant messagin call.
        /// </summary>
        public InstantMessagingCall InstantMessagingCall { get; set; }       
    }
}
