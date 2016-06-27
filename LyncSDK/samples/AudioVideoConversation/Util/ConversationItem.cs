/*===================================================================== 
  This file is part of the Microsoft Unified Communications Code Samples. 

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System;
using System.Diagnostics;
using System.Text;

using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;

namespace AudioVideoConversation
{

    /// <summary>
    /// Helper class for displaying a conversation's participant list as a line in a listbox.
    /// </summary>
    public class ConversationItem
    {
        public ConversationItem(Conversation conversation)
        {
            Debug.Assert(conversation != null); 
            Conversation = conversation;
        }

        /// <summary>
        /// Gets / Sets the Lync Conversation.
        /// </summary>
        public Conversation Conversation { get; private set; }

        /// <summary>
        /// Returns the display name of the contact.
        /// </summary>
        public override string ToString()
        {
            StringBuilder participantNames = new StringBuilder(100);

            //iterates through the participants in the conversation to list their display names
            foreach (Participant participant in Conversation.Participants)
            {
                //ignores the self participant
                if (participant.IsSelf)
                {
                    continue;
                }

                //concatenates the display name into the list
                string name = null;
                try
                {
                    name = participant.Contact.GetContactInformation(ContactInformationType.DisplayName) as string;
                }
                catch (LyncClientException lyncClientException)
                {
                    Console.WriteLine(lyncClientException);
                }
                catch (SystemException systemException)
                {
                    if (LyncModelExceptionHelper.IsLyncException(systemException))
                    {
                        // Log the exception thrown by the Lync Model API.
                        Console.WriteLine("Error: " + systemException);
                    }
                    else
                    {
                        // Rethrow the SystemException which did not come from the Lync Model API.
                        throw;
                    }
                }

                participantNames.Append(name ?? "<Unknown>").Append(", ");
            }

            return participantNames.ToString();
        }
    }

}
