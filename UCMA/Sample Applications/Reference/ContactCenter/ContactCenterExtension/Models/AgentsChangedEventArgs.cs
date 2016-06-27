/*=====================================================================
  File:      AgentsChangedEventArgs.cs

  Summary:   Helper for handling updating and removing Agents.

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
    /// Helper class to update and remove Agents
    /// </summary>
    public class AgentsChangedEventArgs : EventArgs
    {
        #region Private Fields

        private readonly Collection<agentType> _agentsUpdated;
        private readonly Collection<string> _agentsRemoved;

        #endregion

        #region Properties

        public Collection<agentType> AgentsUpdated
        {
            get { return _agentsUpdated; }
        }

        public Collection<string> AgentsRemoved
        {
            get { return _agentsRemoved; }
        }

        #endregion

        #region Internal Constructors

        internal AgentsChangedEventArgs(IEnumerable<agentType> agentsUpdated,
                                        IEnumerable<string> agentsRemoved)
        {
            _agentsUpdated = new Collection<agentType>(new List<agentType>(agentsUpdated));
            _agentsRemoved = new Collection<string>(new List<string>(agentsRemoved));
        }

        #endregion
    }
}