/*=====================================================================
  File:      Agent.cs

  Summary:   Encapsulates agent data, including status.

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
using System.Threading;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.Utilities;



namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{
    /// <summary>
    /// Represents an agent which can receive calls from the system.


    /// </summary>
	public class Agent
	{
        private bool _allocated;
        private AcdCustomerSession _owner;
        private DateTime _activeIdleSince;
        private bool _havePropertiesChanged = false;
        private AcdLogger _logger;
        private AgentAllocationStatus _allocationStatus = AgentAllocationStatus.NotAllocated;
        private AsyncResult<AgentHuntResult> _asyncResult;

        private object _syncRoot = new object();

        internal Agent(AcdLogger logger)
        {
            _logger = logger;
            this.IsOnline = false;
        }


        /// <summary>
        /// The address which the agent uses to sign in.
        /// </summary>
        internal string SignInAddress
        {
            get;
            set;
        }

        /// <summary>
        /// The name used to identify the agent externally.
        /// </summary>
        internal string PublicName
        {
            get;
            set;
        }

        /// <summary>
        /// The set of skills an agent possesses.
        /// </summary>
        internal List<AgentSkill> Skills
        {
            get;
            set;
        }
        /// <summary>
        /// Determines if an agent has a particular skill.
        /// </summary>
        /// <param name="skill">The skill in question.</param>
        /// <returns>True if the agent has the skilil.</returns>
        internal bool HasSkill(AgentSkill skill)
        {
            return this.Skills.Contains(skill);
        }

        /// <summary>
        /// Indicates whether an Agent is serving a customer.
        /// </summary>
        internal bool IsAllocated
        {
            get { return _allocated; }
        }

        internal AgentAllocationStatus AllocationStatus
        {
            get { return _allocationStatus; }

            set { _allocationStatus = value;  }
        }

        internal AsyncResult<AgentHuntResult> AsyncResult
        {
            get { return _asyncResult; }
            set { _asyncResult = value; }
        }


        /// <summary>
        /// Gets the owner of the agent
        /// </summary>
        internal AcdCustomerSession Owner
        {
            get { return _owner; }
        }

        internal string InstantMessageColor
        {
            get;
            set;
        }

        /// <summary>
        /// Allocates the agent so that she cannot be allocated twice.
        /// </summary>
        internal void Allocate(AcdCustomerSession requestor)
        {
            lock (_syncRoot)
            {
                if (_allocated)
                {
                    throw new InvalidOperationException("Agent is already allocated.");
                }
                if (requestor == null)
                {
                    throw new InvalidOperationException("Agent Requestor cannot be null.");
                }
                Debug.Write("Agent " + this.SignInAddress.ToString() + "was allocated to" + requestor.ToString());

                _owner = requestor;
                _allocated = true;
                _activeIdleSince = DateTime.UtcNow;
                _havePropertiesChanged = true;
                _allocationStatus = AgentAllocationStatus.AllocatedByMatchMaker;
            }
        }



        /// <summary>
        /// Releases the agent so that she can be allocated subsequently.
        /// </summary>
        internal void Deallocate(object owner)
        {
            lock (_syncRoot)
            {
                if (!_allocated)
                {
                    _logger.Log("Agent is not allocated.");
                }
                else if (_owner == owner) // we verify that only the one, that allocated the agent, deallocates her
                {
                    _logger.Log("Agent " + this.SignInAddress.ToString() + "was deallocated by " + owner.ToString()  );
                    _allocated = false;
                    _allocationStatus = AgentAllocationStatus.NotAllocated;
                    _activeIdleSince = DateTime.UtcNow;
                    _asyncResult = null;
                    _owner = null;
                    _havePropertiesChanged = true;
                }
                else
                {
                    _logger.Log("Agent can only be deallocated by its owner");
                }
            }
        }

        internal bool GetWhetherPropertiesChanged()
        {
            bool havePropertiesChanged;

            lock (_syncRoot)
            { 
              havePropertiesChanged = _havePropertiesChanged;
              _havePropertiesChanged = false;
              return havePropertiesChanged;
            }
        }

        internal KeyValuePair<DateTime, bool> GetWhetherAllocated()
        { 
          lock(_syncRoot)
          {
            return new KeyValuePair<DateTime,Boolean>(_activeIdleSince, _allocated);
          }
        }


        /// <summary>
        /// Determines if an agent has all of a list of skills.
        /// </summary>
        /// <param name="skills">The list of skills in question.</param>
        /// <returns>True if the agent has all the specified skills.</returns>
        internal bool HasSkills(List<AgentSkill> skills)
        {
            foreach (AgentSkill agentSkill in skills)
            {
                if (!this.HasSkill(agentSkill))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool IsOnline
        {
            get;
            set;
        }

        public Supervisor Supervisor
        {
            get;
            set;
        }

        public string SupervisorUri
        {
            get;
            set;
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

        static public agentType Convert(Agent agent, ConversationParticipant participant)
        {
            agentType agentT = new agentType();
            if (null != participant)
            {
                agentT.displayname = participant.DisplayName;
                agentT.uri = participant.Uri;

                int numberOfActiveModalities = participant.GetActiveMediaTypes().Count;

                if (numberOfActiveModalities != 0)
                {
                    agentT.mediatypes = new string[numberOfActiveModalities];
                    
                    int i = 0;
                    foreach (String mediaType in participant.GetActiveMediaTypes())
                    {
                        agentT.mediatypes[i++] = mediaType;

                    }
                }
            }
            else
            {
                agentT.displayname = new SipUriParser(agent.SignInAddress).User;
                agentT.uri = agent.SignInAddress;
            }
            KeyValuePair<DateTime, bool> allocated = agent.GetWhetherAllocated();
            agentT.status = allocated.Value ? "Active" : "Idle";
            agentT.statuschangedtime = allocated.Key.Ticks.ToString();

            return agentT;
        
               
        }


    }


    internal enum AgentAllocationStatus {NotAllocated = 0,AllocatedByMatchMaker=1, CommittingTheAgent=2, EscalatingTheAgent=3, AssigningTheAgentToItsOwner=4 }
}
