/*=====================================================================
  File:      Supervisor.cs

  Summary:   Represents a supervisor.


/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Diagnostics;



namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// Represents an agent which can receive calls from the system.
    /// </summary>
    public class Supervisor
    {
        private object _syncRoot = new object();
        private string _uri;
        private string _publicName;
        private string _instantMessageColor;
        private List<Agent> _listOfAgents = new List<Agent>();


        internal Supervisor()
        {

        }


        /// <summary>
        /// The address which the supervisor uses to sign in.
        /// </summary>
        internal string SignInAddress
        {
            get {return _uri;}
            set { _uri = value; }
        }

        /// <summary>
        /// The name used to identify the supervisor externally.
        /// </summary>
        internal string PublicName
        {
            get { return _publicName; }
            set { _publicName = value; }

        }

        /// <summary>
        /// The Agents assigned to the Supervisor.
        /// </summary>
        internal List<Agent> Agents
        {
            get {return _listOfAgents;}
        }


        internal string InstantMessageColor
        {
            get {return _instantMessageColor;}
            set { _instantMessageColor = value; }
        }

        public override bool Equals(object obj)
        {
            Agent agent = obj as Agent;
            if (agent == null)
            {
                return false;
            }
            if (!SipUriCompare.Equals(this.SignInAddress, agent.SignInAddress))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return this.SignInAddress.GetHashCode();
        }

        public override string ToString()
        {
            return this.SignInAddress;
        }
    }
}
