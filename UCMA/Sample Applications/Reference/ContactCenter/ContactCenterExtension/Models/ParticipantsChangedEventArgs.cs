/*=====================================================================
  File:      ParticipantsChangedEventArgs.cs

  Summary:   Model for handling the task of updating and removing
            Participants.

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    /// <summary>
    /// Helper class to update and remove Participants
    /// </summary>
    public class ParticipantsChangedEventArgs : EventArgs
    {
        #region Public Properties

        public Collection<participantType> ParticipantsUpdated { get; private set; }
        public Collection<string> ParticipantsRemoved { get; private set; }

        #endregion

        #region Internal Constructors

        internal ParticipantsChangedEventArgs(IEnumerable<participantType> participantsUpdated,
                                              IEnumerable<string> participantsRemoved)
        {
            ParticipantsUpdated = new Collection<participantType>(new List<participantType>(participantsUpdated));
            ParticipantsRemoved = new Collection<string>(new List<string>(participantsRemoved));
        }

        #endregion
    }
}