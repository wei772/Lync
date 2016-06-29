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
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using GalaSoft.MvvmLight;

namespace BlueOfficeLync.Model
{
    /// <summary>
    /// Helper class for displaying a participant's name in a listbox.
    /// </summary>
    public class ParticipantModel : ObservableObject
	{
        public ParticipantModel(Participant participant)
        {
            Debug.Assert(participant != null);
            Participant = participant;
        }

        /// <summary>
        /// Gets / Sets the Lync Participant.
        /// </summary>
        public Participant Participant { get; private set; }

        /// <summary>
        /// Returns the display name of the contact.
        /// </summary>
        public override string ToString()
        {
            //in case ToString() is called before the participant is set
            if (Participant == null)
            {
                return string.Empty;
            }

            //obtains the display name of the participant
            string displayName = null;
            try
            {
                displayName = Participant.Contact.GetContactInformation(ContactInformationType.DisplayName) as string;
            }
            catch (LyncClientException lyncClientException)
            {
                Console.WriteLine(lyncClientException);
            }
            catch (SystemException systemException)
            {
                //if (LyncModelExceptionHelper.IsLyncException(systemException))
                //{
                //    // Log the exception thrown by the Lync Model API.
                //    Console.WriteLine("Error: " + systemException);
                //}
                //else
                //{
                //    // Rethrow the SystemException which did not come from the Lync Model API.
                //    throw;
                //}
            }


            //if the contact is self, then add a (self) sufix
            displayName = displayName ?? "<Unknown>";
            return Participant.IsSelf ? displayName + " (self)" : displayName;
        }
    }


}
