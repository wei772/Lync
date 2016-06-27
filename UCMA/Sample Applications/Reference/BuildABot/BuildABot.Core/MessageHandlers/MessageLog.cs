namespace BuildABot.Core.MessageHandlers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using BuildABot.Core.MessageHandlers;

    /// <summary>
    /// Keeps track of all conversations happening in the Bot for last 24 hours.
    /// </summary>
    public static class MessageLog
    {
        /// <summary>
        /// Keep log for last 24 hours only.
        /// </summary>
        private const double MessagesLogLiveTimeInHours = 24;

        /// <summary>
        /// Keeps information about all available messages.
        /// </summary>
        private static List<Message> messageCollection = new List<Message>();

        /// <summary>
        /// Adds the message to log.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void AddMessageToLog(Message message)
        {
            if (message != null)
            {
                messageCollection.Add(message);
            }
            ClearOldMessages();
        }

        /// <summary>
        /// Adds the message to log.
        /// </summary>
        /// <param name="content">The content.</param>
        public static void AddMessageToLog(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                Message message = new Message(content);
                AddMessageToLog(message);
            }
            ClearOldMessages();
        }

        /// <summary>
        /// Adds the message to log.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="senderName">Name of the sender.</param>
        public static void AddMessageToLog(string content, string senderName)
        {
            if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(senderName))
            {
                Message message = new Message(content, senderName, senderName, DateTime.Now, null, null);
                AddMessageToLog(message);
            }
            ClearOldMessages();
        }

        /// <summary>
        /// Gets the conference uri.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetConferenceUris()
        {
            return messageCollection.Select(message => message.ConferenceUri).Distinct();
        }

        /// <summary>
        /// Gets the conference ids.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetConversationIds()
        {
            return messageCollection.Select(message => message.ConversationId).Distinct();
        }

        /// <summary>
        /// Gets the conference fully formatted conference URLs.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetConferenceUrls()
        {
            return messageCollection.Where(message => !string.IsNullOrWhiteSpace(message.ConversationId) && 
                                                      !string.IsNullOrWhiteSpace(message.ConferenceUri)).
                                     Select(message => string.Format(CultureInfo.InvariantCulture,
                                                       "conf:{0}%3Fconversation-id={1}",
                                                       message.ConferenceUri, message.ConversationId)).Distinct();
        }


        #region Get Messages overloads

        /// <summary>
        /// Gets the messages from the log.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Message> GetMessages()
        {
            return messageCollection;
        }

        /// <summary>
        /// Gets the messages from the log.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public static IEnumerable<Message> GetMessages(DateTime startTime, DateTime endTime)
        {
            return messageCollection.Where(message =>
            DateTime.Compare(message.TimeStamp, startTime) >= 0 &&
            DateTime.Compare(message.TimeStamp, endTime) <= 0);
        }

        /// <summary>
        /// Gets the messages from the log for the specified query parameter.
        /// </summary>
        /// <param name="queryParameter">The query parameter where key is parameter name</param>
        /// <returns></returns>
        public static IEnumerable<Message> GetMessages(KeyValuePair<MessageLogParameters, string> queryParameter)
        {
            if (!string.IsNullOrWhiteSpace(queryParameter.Value))
            {
                switch (queryParameter.Key)
                {
                    case MessageLogParameters.ConferenceUri:
                        return messageCollection.Where(message =>
                        message.ConferenceUri.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase));

                    case MessageLogParameters.ConversationId:
                        return messageCollection.Where(message =>
                       message.ConversationId.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase));

                    case MessageLogParameters.UserAlias:
                        return messageCollection.Where(message =>
                        message.SenderAlias.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase));

                    case MessageLogParameters.UserName:
                        return messageCollection.Where(message =>
                       message.SenderDisplayName.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase));

                    default: break;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the messages from the log for the specified query parameter.
        /// </summary>
        /// <param name="queryParameter">The query parameter where key is parameter name</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public static IEnumerable<Message> GetMessages(KeyValuePair<MessageLogParameters, string> queryParameter, DateTime startTime, DateTime endTime)
        {
            if (!string.IsNullOrWhiteSpace(queryParameter.Value))
            {
                switch (queryParameter.Key)
                {
                    case MessageLogParameters.ConferenceUri:
                        return messageCollection.Where(message =>
                        message.ConferenceUri.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.Compare(message.TimeStamp, startTime) >= 0 &&
                        DateTime.Compare(message.TimeStamp, endTime) <= 0);

                    case MessageLogParameters.ConversationId:
                        return messageCollection.Where(message =>
                        message.ConversationId.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.Compare(message.TimeStamp, startTime) >= 0 &&
                        DateTime.Compare(message.TimeStamp, endTime) <= 0);

                    case MessageLogParameters.UserAlias:
                        return messageCollection.Where(message =>
                        message.SenderAlias.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.Compare(message.TimeStamp, startTime) >= 0 &&
                        DateTime.Compare(message.TimeStamp, endTime) <= 0);

                    case MessageLogParameters.UserName:
                        return messageCollection.Where(message =>
                        message.SenderDisplayName.Equals(queryParameter.Value, StringComparison.OrdinalIgnoreCase) &&
                        DateTime.Compare(message.TimeStamp, startTime) >= 0 &&
                        DateTime.Compare(message.TimeStamp, endTime) <= 0);

                    default: break;
                }
            }
            return null;
        }
        #endregion

        #region support Methods
        /// <summary>
        /// Clears the old messages.
        /// </summary>
        private static void ClearOldMessages()
        {
            // Select messages with timestamp older than 24 hours
            IEnumerable<Message> oldMessages = messageCollection.Where(message =>
                DateTime.Compare(message.TimeStamp, DateTime.Now.AddHours(-1 * MessagesLogLiveTimeInHours)) < 0);

            // TODO Instead of deleting them save to file ?
            if (oldMessages != null && oldMessages.Count() > 0)
            {
                for (int index = 0; index < oldMessages.Count(); index++)
                {
                    messageCollection.Remove(oldMessages.ElementAt(index));
                    index--;
                }
            }
        }
        #endregion
    }
}
