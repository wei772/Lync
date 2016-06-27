/*=====================================================================
  File:      SupervisorMonitoringChannel.cs

  Summary:   The model for supervisor's conversation view. 

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
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;


namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    /// <summary>
    /// The model for the supervisor monitoring view.
    /// </summary>
    public class SupervisorMonitoringChannel
    {
        #region Private Fields

        private readonly Dictionary<string, agentType> _agents;
        private readonly Dictionary<string, participantType> _participants;
        private readonly Dictionary<string, participantType> _customers;
        private readonly RequestProcessor _requestProcessor;

        #endregion

        #region Public Properties

        public string SessionId { get; private set; }

        public SupervisorDashboardChannel SupervisorDashboardChannel { get; private set; }

        public Collection<agentType> Agents
        {
            get
            {
                lock (_agents)
                {
                    return new Collection<agentType>(_agents.Values.ToArray());
                }
            }
        }

        public Collection<participantType> Participants
        {
            get
            {
                lock (_participants)
                {
                    return new Collection<participantType>(_participants.Values.ToArray());
                }
            }
        }

        public Collection<participantType> Customers
        {
            get
            {
                lock (_customers)
                {
                    return new Collection<participantType>(_customers.Values.ToArray());
                }
            }
        }

        #endregion

        #region Public Events

        public event EventHandler<AgentsChangedEventArgs> AgentsChanged;
        public event EventHandler<ParticipantsChangedEventArgs> ParticipantsChanged;
        public event EventHandler<ParticipantsChangedEventArgs> CustomersChanged;

        #endregion

        #region Constructors

        internal SupervisorMonitoringChannel(SupervisorDashboardChannel channel, string sessionId)
        {
            SupervisorDashboardChannel = channel;
            SessionId = sessionId;
            _requestProcessor = new RequestProcessor(sessionId);

            _agents = new Dictionary<string, agentType>();
            _participants = new Dictionary<string, participantType>();
            _customers = new Dictionary<string, participantType>();
        }

        #endregion

        #region Public Methods

        public IAsyncResult BeginBargeIn(
            AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.BargeIn);


            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, SupervisorDashboardChannel.Conversation, SupervisorDashboardChannel.ApplicationId,
                userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public void EndBargeIn(IAsyncResult result)
        {
            ProcessRequestAsyncResult asyncResult = result as ProcessRequestAsyncResult;

            if (asyncResult != null)
            {
                asyncResult.EndInvoke();
            }
            else
            {
                throw new Exception("Invalid async result");
            }

        }

        public IAsyncResult BeginWhisper(Uri uri,
            AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.Whisper);

            request.whisper = new whisperType();
            request.whisper.uri = uri.ToString();

            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, SupervisorDashboardChannel.Conversation, SupervisorDashboardChannel.ApplicationId,
                userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public void EndWhisper(IAsyncResult result)
        {
            ProcessRequestAsyncResult asyncResult = result as ProcessRequestAsyncResult;

            if (asyncResult != null)
            {
                asyncResult.EndInvoke();
            }
            else
            {
                throw new Exception("Invalid async result");
            }

        }

        #endregion

        #region Internal Methods

        internal void ProcessResponse(responseType response)
        {
            _requestProcessor.ProcessResponse(response);
        }

        internal void ProcessNotification(notificationType notification)
        {
            if (notification.agentinfos.Length > 0 || notification.agentsremoved.Length > 0)
            {
                lock (_agents)
                {
                    foreach (agentType notificationAgentinfo in notification.agentinfos)
                    {
                        _agents[notificationAgentinfo.uri] = notificationAgentinfo;
                    }

                    foreach (string uri in notification.agentsremoved)
                    {
                        _agents.Remove(uri);
                    }
                }

                AgentsChangedEventArgs e = new AgentsChangedEventArgs(notification.agentinfos, notification.agentsremoved);

                EventHandler<AgentsChangedEventArgs> agentsChangedHandler = AgentsChanged;

                if (agentsChangedHandler != null)
                {
                    agentsChangedHandler(this, e);
                }


            }

            if (notification.participantinfos.Length > 0 || notification.partcicipantsremoved.Length > 0)
            {
                List<participantType> customersUpdated = new List<participantType>();
                List<participantType> participantsUpdated = new List<participantType>();
                List<string> customersRemoved = new List<string>();
                List<string> participantsRemoved = new List<string>();

                lock (_participants)
                {
                    foreach (participantType notificationParticipantinfo in notification.participantinfos)
                    {
                        if (notificationParticipantinfo.iscustomer)
                        {
                            _customers[notificationParticipantinfo.uri] = notificationParticipantinfo;
                            customersUpdated.Add(notificationParticipantinfo);
                        }
                        else
                        {
                            _participants[notificationParticipantinfo.uri] = notificationParticipantinfo;
                            participantsUpdated.Add(notificationParticipantinfo);
                        }
                    }

                    foreach (string uri in notification.partcicipantsremoved)
                    {
                        if (_customers.Remove(uri))
                        {
                            customersRemoved.Add(uri);
                        }

                        if (_participants.Remove(uri))
                        {
                            participantsRemoved.Add(uri);
                        }
                    }

                }

                if (participantsRemoved.Count > 0 || participantsUpdated.Count > 0)
                {
                    ParticipantsChangedEventArgs e = new ParticipantsChangedEventArgs(participantsUpdated, participantsRemoved);

                    EventHandler<ParticipantsChangedEventArgs> participantsChangedHandler = ParticipantsChanged;

                    if (participantsChangedHandler != null)
                    {
                        participantsChangedHandler(this, e);
                    }
                }

                if (customersRemoved.Count > 0 || customersUpdated.Count > 0)
                {
                    ParticipantsChangedEventArgs e = new ParticipantsChangedEventArgs(customersUpdated, customersRemoved);

                    EventHandler<ParticipantsChangedEventArgs> customersChangedHandler = CustomersChanged;

                    if (customersChangedHandler != null)
                    {
                        customersChangedHandler(this, e);
                    }
                }
            }
        }

        #endregion
    }

}
