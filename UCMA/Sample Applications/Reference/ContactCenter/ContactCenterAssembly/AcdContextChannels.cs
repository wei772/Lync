/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Collections.Generic;
using Microsoft.Rtc.Collaboration;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Microsoft.Rtc.Signaling;
using System.Text;
using System.Diagnostics;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter
{

    public class AgentContextChannel
    {
        #region private members

        private ConversationContextChannel m_innerChannel;

        #endregion

        public AgentContextChannel(Conversation conversation, ParticipantEndpoint remoteEndpoint)
        {
            m_innerChannel = new ConversationContextChannel(conversation, remoteEndpoint);
        }

        public IAsyncResult BeginEstablish(
                string conversationId, 
                Guid applicationId,
                productType productInfo,
                IEnumerable<skillType> skills,
                AsyncCallback userCallback, object state)
        {
            agentDashboardInitType agentdashboardinitdata = new agentDashboardInitType();
            agentdashboardinitdata.product = productInfo;
            agentdashboardinitdata.skills = new List<skillType>(skills).ToArray();


            string serializedInitData;

            XmlSerializer serializer = new XmlSerializer(agentdashboardinitdata.GetType());

            using (TextWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, agentdashboardinitdata);
                serializedInitData = writer.ToString();
            }

            Console.WriteLine(serializedInitData);

            ConversationContextChannelEstablishOptions options = new ConversationContextChannelEstablishOptions();

            options.ContextualData = serializedInitData;
            options.Toast = "hi";
            options.RemoteConversationId = conversationId;

            m_innerChannel.DataReceived += InnerChannel_DataReceived;

            return m_innerChannel.BeginEstablish(applicationId, options, userCallback, state);
        }

        public void EndEstablish(IAsyncResult result)
        {
            m_innerChannel.EndEstablish(result);
        }

        public IAsyncResult BeginTerminate(AsyncCallback userCallback, object state)
        {
            return m_innerChannel.BeginTerminate(userCallback, state);
        }

        public void EndTerminate(IAsyncResult result)
        {
            m_innerChannel.EndTerminate(result);
        }

        public ConversationContextChannelState State
        {
            get { return m_innerChannel.State; }
        }


        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;
        public event EventHandler<ConversationContextChannelStateChangedEventArgs> StateChanged
        {
            add { m_innerChannel.StateChanged += value; }
            remove { m_innerChannel.StateChanged -= value; }
        }

        #region private methods

        private void InnerChannel_DataReceived(object sender, ConversationContextChannelDataReceivedEventArgs e)
        {
            try
            {
                if (e.ContentDescription != null)
                {
                    if (e.ContentDescription.ContentType.ToString().Equals(WireHelpers.Request, StringComparison.OrdinalIgnoreCase))
                    {
                        requestType request = WireHelpers.ParseMessageBody<requestType>(e.ContentDescription.GetBody());
                        this.ProcessRequest(request);
                    }
                    else
                    {
                        //TODO:Log and ignore
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Log and ignore
            }
        }

        private void ProcessRequest(requestType request)
        {
            Debug.Assert(request != null);

            ContextChannelRequestReceivedEventArgs eventArgs = null;
            ContextChannelRequest contextChannelRequest = null;

            if (request.hold != null)
            {
                contextChannelRequest = new ContextChannelRequest(request,
                    ContextChannelRequestType.Hold,
                    m_innerChannel);
            }
            else if (request.escalate != null)
            {
                contextChannelRequest = new EscalateRequest(request,
                    m_innerChannel);
            }
            else if (request.retrieve != null)
            {
                contextChannelRequest = new ContextChannelRequest(request,
                    ContextChannelRequestType.Retrieve,
                    m_innerChannel);
            }
            else
            {
                //TODO:Log and ignore
            }

            eventArgs = new ContextChannelRequestReceivedEventArgs(contextChannelRequest.RequestType, contextChannelRequest);

            EventHandler<ContextChannelRequestReceivedEventArgs> eventHandler = this.RequestReceived;
            if (eventHandler != null)
            {
                eventHandler.Invoke(this, eventArgs);
            }

        }


        #endregion
    }

    public class SupervisorContextChannel
    {
        #region private members

        private ConversationContextChannel m_innerChannel;
        private MonitoringChannel m_monitoringChannel;

        #endregion

        public SupervisorContextChannel(Conversation conversation, ParticipantEndpoint remoteEndpoint)
        {
            m_innerChannel = new ConversationContextChannel(conversation, remoteEndpoint);
        }

        #region public methods
        public IAsyncResult BeginEstablish(Guid applicationId,
                AsyncCallback userCallback, object state)
        {
            ConversationContextChannelEstablishOptions options = new ConversationContextChannelEstablishOptions();

            options.Toast = "hi";

            m_innerChannel.DataReceived += InnerChannel_DataReceived;

            return m_innerChannel.BeginEstablish(applicationId, options, userCallback, state);
        }

        public void EndEstablish(IAsyncResult result)
        {
            m_innerChannel.EndEstablish(result);
        }

        public IAsyncResult BeginTerminate(AsyncCallback userCallback, object state)
        {
            return m_innerChannel.BeginTerminate(userCallback, state);
        }

        public void EndTerminate(IAsyncResult result)
        {
            m_innerChannel.EndTerminate(result);
        }

        public IAsyncResult BeginUpdateAgents(IEnumerable<agentType> agentsAddedOrModified,
            IEnumerable<string> agentsRemoved,
            AsyncCallback callback, object state)
        {
            return this.BeginSendNotification(agentsAddedOrModified, agentsRemoved, null, null,
                null, callback, state);
        }

        public void EndUpdateAgents(IAsyncResult result)
        {
            this.EndSendNotification(result);
        }

        #endregion

        #region public properties

        public ConversationContextChannelState State
        {
            get { return m_innerChannel.State; }
        }

        public ConversationContextChannel InnerChannel
        {
            get { return m_innerChannel; }
        }

        #endregion

        #region public events

        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;
        public event EventHandler<ConversationContextChannelStateChangedEventArgs> StateChanged
        {
            add { m_innerChannel.StateChanged += value; }
            remove { m_innerChannel.StateChanged -= value; }
        }

        #endregion


        #region private methods

        internal IAsyncResult BeginSendNotification(IEnumerable<agentType> agentsAddedOrModified,
            IEnumerable<string> agentsRemoved,
            IEnumerable<participantType> participantsAddedOrModified,
            IEnumerable<string> participantsRemoved,
            string sessionId,
            AsyncCallback callback, object state)
        {
            notificationType notification = new notificationType();
            notification.sessionId = sessionId;

            if (agentsAddedOrModified != null)
            {
                List<agentType> updatedAgents = new List<agentType>(agentsAddedOrModified);
                notification.agentinfos = updatedAgents.ToArray();
            }

            if (agentsRemoved != null)
            {
                List<string> removedAgents = new List<string>(agentsRemoved);
                notification.agentsremoved = removedAgents.ToArray();
            }

            if (participantsAddedOrModified != null)
            {
                List<participantType> updatedParticipants = new List<participantType>(participantsAddedOrModified);
                notification.participantinfos = updatedParticipants.ToArray();
            }

            if (participantsRemoved != null)
            {
                List<string> removedParticipants = new List<string>(participantsRemoved);
                notification.partcicipantsremoved = removedParticipants.ToArray();
            }

            string serializedData;

            XmlSerializer serializer = new XmlSerializer(notification.GetType());

            using (TextWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, notification);
                serializedData = writer.ToString();
            }

            Console.WriteLine(serializedData);

            return m_innerChannel.BeginSendData(new System.Net.Mime.ContentType(WireHelpers.Notification), System.Text.Encoding.UTF8.GetBytes(serializedData),
                callback, state);
        }

        internal void EndSendNotification(IAsyncResult result)
        {
            m_innerChannel.EndSendData(result);
        }

        private void InnerChannel_DataReceived(object sender, ConversationContextChannelDataReceivedEventArgs e)
        {
            try
            {
                if (e.ContentDescription != null)
                {
                    if (e.ContentDescription.ContentType.ToString().Equals(WireHelpers.Request, StringComparison.OrdinalIgnoreCase))
                    {
                        requestType request = WireHelpers.ParseMessageBody<requestType>(e.ContentDescription.GetBody());
                        this.ProcessRequest(request);
                    }
                    else
                    {
                        //TODO:Log and ignore
                    }
                }
            }
            catch (Exception)
            {
                //TODO: Log and ignore
            }
        }

        private void ProcessRequest(requestType request)
        {
            Debug.Assert(request != null);

            ContextChannelRequestReceivedEventArgs eventArgs = null;
            ContextChannelRequest contextChannelRequest = null;

            MonitoringChannel mChannel = m_monitoringChannel;

            if (!string.IsNullOrEmpty(request.sessionId) &&
                mChannel != null &&
                mChannel.Id.Equals(request.sessionId, StringComparison.OrdinalIgnoreCase))
            {
                mChannel.ProcessRequest(request);
            }
            else
            {
                if (request.startmonitoringsession != null)
                {
                    m_monitoringChannel = new MonitoringChannel(request.startmonitoringsession.sessionId,
                        new Uri(request.startmonitoringsession.uri), this);

                    contextChannelRequest = new MonitoringRequest(request,ContextChannelRequestType.StartMonitoring,  m_monitoringChannel);
                }
                else if (request.terminatemonitoringsession != null)
                {
                    MonitoringChannel monitoringChannel = m_monitoringChannel;

                    if (monitoringChannel != null &&
                        String.Equals(request.terminatemonitoringsession.sessionId, monitoringChannel.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        contextChannelRequest = new MonitoringRequest(request, ContextChannelRequestType.StopMonitoring, monitoringChannel);
                    }
                }
                else
                {
                    //TODO:Log and ignore
                }

                if (contextChannelRequest != null)
                {
                    eventArgs = new ContextChannelRequestReceivedEventArgs(contextChannelRequest.RequestType, contextChannelRequest);

                    EventHandler<ContextChannelRequestReceivedEventArgs> eventHandler = this.RequestReceived;
                    if (eventHandler != null)
                    {
                        eventHandler.Invoke(this, eventArgs);
                    }
                }
            }

        }


        #endregion


    }

    public class MonitoringChannel
    {
        #region private data members

        private string m_id;
        private Uri m_uri;
        private SupervisorContextChannel m_supervisorChannel;

        #endregion

        #region public properties
        public String Id
        {
            get { return m_id; }
        }

        public Uri Uri
        {
            get { return m_uri; }
        }

        public SupervisorContextChannel SupervisorContextChannel
        {
            get { return m_supervisorChannel; }
        }

        #endregion

        public MonitoringChannel(string id, Uri uri, SupervisorContextChannel supervisorChannel)
        {
            m_id = id;
            m_uri = uri;
            m_supervisorChannel = supervisorChannel;
        }

        #region public methods

        public IAsyncResult BeginUpdateParticipants(IEnumerable<participantType> participantsAddedOrModified,
            IEnumerable<string> participantsRemoved,
            AsyncCallback callback, object state)
        {
            return m_supervisorChannel.BeginSendNotification(null, null, participantsAddedOrModified, participantsRemoved,
                m_id, callback, state);
        }
        public void EndUpdateParticipants(IAsyncResult result)
        {
            m_supervisorChannel.EndSendNotification(result);
        }

        public IAsyncResult BeginUpdateAgents(IEnumerable<agentType> agentsAddedOrModified,
            IEnumerable<string> agentsRemoved,
            AsyncCallback callback, object state)
        {
            return m_supervisorChannel.BeginSendNotification(agentsAddedOrModified, agentsRemoved, null, null, m_id,
                callback, state);
        }
        public void EndUpdateAgents(IAsyncResult result)
        {
            m_supervisorChannel.EndSendNotification(result);
        }

        #endregion

        #region public events
        public event EventHandler<ContextChannelRequestReceivedEventArgs> RequestReceived;

        #endregion

        #region private methods
        internal void ProcessRequest(requestType request)
        {
            Debug.Assert(request != null);

            ContextChannelRequestReceivedEventArgs eventArgs = null;
            ContextChannelRequest contextChannelRequest = null;

            if (request.bargein != null)
            {
                contextChannelRequest = new ContextChannelRequest(request,
                    ContextChannelRequestType.BargeIn,
                    m_supervisorChannel.InnerChannel);
            }
            else if (request.whisper != null)
            {
                contextChannelRequest = new WhisperRequest(request,
                    m_supervisorChannel.InnerChannel);
            }
            //else if (request.terminatemonitoringsession != null)
            //{
            //    contextChannelRequest = new ContextChannelRequest(request,
            //        ContextChannelRequestType.Hold,
            //        m_innerChannel);
            //}
            else
            {
                //TODO:Log and ignore
            }

            eventArgs = new ContextChannelRequestReceivedEventArgs(contextChannelRequest.RequestType, contextChannelRequest);

            EventHandler<ContextChannelRequestReceivedEventArgs> eventHandler = this.RequestReceived;
            if (eventHandler != null)
            {
                eventHandler.Invoke(this, eventArgs);
            }

        }

        #endregion
    }




    public class ContextChannelRequest
    {
        #region private members

        private requestType m_request;
        private ContextChannelRequestType m_requestType;
        private ConversationContextChannel m_channel;

        #endregion

        public ContextChannelRequest(requestType request,
            ContextChannelRequestType requestType,
            ConversationContextChannel channel)
        {
            Debug.Assert(request != null);
            Debug.Assert(channel != null);

            m_request = request;
            m_channel = channel;
            m_requestType = requestType;
        }

        public ContextChannelRequestType RequestType
        {
            get { return m_requestType; }
        }

        public void SendResponse(string responseCode)
        {

            responseType response = new responseType();
            response.requestId = m_request.requestId;
            response.result = responseCode;
            response.sessionId = m_request.sessionId;

            Console.WriteLine("RESPONSE GETTING SENT " + responseCode + " " + response.requestId.ToString());

            string responseData;

            XmlSerializer serializer = new XmlSerializer(response.GetType());

            using (TextWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, response);
                responseData = writer.ToString();
            }

            m_channel.BeginSendData(
                new System.Net.Mime.ContentType(WireHelpers.Response),
                new ASCIIEncoding().GetBytes(responseData),
                (r) =>
                {
                    try
                    {
                        m_channel.EndSendData(r);
                    }
                    catch (Exception)
                    {
                        //TODO:Log and Ignore
                    }
                }, null);

        }
    }

    public class ContextChannelRequestReceivedEventArgs : EventArgs
    {
        #region private members

        private ContextChannelRequestType m_requestType;
        public ContextChannelRequest m_request;

        #endregion

        internal ContextChannelRequestReceivedEventArgs(ContextChannelRequestType requestType,
            ContextChannelRequest request)
            : base()
        {
            Debug.Assert(request != null);

            m_request = request;
            m_requestType = requestType;
        }

        public ContextChannelRequestType RequestType
        {
            get { return m_requestType; }
        }

        public ContextChannelRequest Request
        {
            get { return m_request; }
        }
    }

    public class WhisperRequest : ContextChannelRequest
    {
        private Uri m_uri;

        public WhisperRequest(requestType request,
            ConversationContextChannel channel)
            : base(request, ContextChannelRequestType.Whisper, channel)
        {
            m_uri = new Uri(request.whisper.uri);
        }

        public Uri Uri
        {
            get { return m_uri; }
        }
    }

    public class EscalateRequest : ContextChannelRequest
    {
        private List<agentSkillType> m_skills;

        public EscalateRequest(requestType request,
            ConversationContextChannel channel)
            : base(request, ContextChannelRequestType.Escalate, channel)
        {
            m_skills = new List<agentSkillType>(request.escalate.agentSkills);
        }

        public Collection<agentSkillType> Skills
        {
            get { return new Collection<agentSkillType>(m_skills); }
        }
    }

    public class MonitoringRequest : ContextChannelRequest
    {
        private MonitoringChannel m_monitoringChannel;

        internal MonitoringRequest(requestType request, ContextChannelRequestType requestType, MonitoringChannel monitoringChannel)
            : base(request, requestType, monitoringChannel.SupervisorContextChannel.InnerChannel)
        {
            m_monitoringChannel = monitoringChannel;
        }

        public MonitoringChannel MonitoringChannel
        {
            get { return m_monitoringChannel; }
        }
    }

    public class WireHelpers
    {
        public const string Request = "application/ContactCenterRequest+xml";
        public const string Response = "application/ContactCenterResponse+xml";
        public const string Notification = "application/ContactCenterNotification+xml";

        public static T ParseMessageBody<T>(byte[] body) where T : class
        {
            T parsedObject = null;

            if (body != null)
            {
                string data = System.Text.Encoding.UTF8.GetString(body);

                XmlSerializer serializer = new XmlSerializer(typeof(T));

                using (XmlTextReader reader = new XmlTextReader(new StringReader(data)))
                {
                    parsedObject = (T)serializer.Deserialize(reader);
                }
            }

            return parsedObject;
        }
    }


    public enum ContextChannelRequestType
    {
        Hold,
        Retrieve,
        Escalate,
        Whisper,
        BargeIn,
        StartMonitoring,
        StopMonitoring
    }
}


