/*=====================================================================
  File:      WireHelpers.cs

  Summary:   Model class for preparing messages and notifications. 

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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    internal class WireHelpers
    {
        #region Constants

        public const string Request = "application/ContactCenterRequest+xml";
        public const string Response = "application/ContactCenterResponse+xml";
        public const string Notification = "application/ContactCenterNotification+xml";

        #endregion

        #region Public Methods

        public static string Serialize(requestType request)
        {
            string serializedRequest;

            using (StringWriter sw = new StringWriter(new StringBuilder(128), CultureInfo.InvariantCulture))
            {
                using (XmlWriter messageWriter = XmlWriter.Create(sw))
                {
                    messageWriter.WriteStartElement("request");

                    messageWriter.WriteStartAttribute("requestId");
                    messageWriter.WriteString(request.requestId.ToString());
                    messageWriter.WriteEndAttribute();

                    if (!string.IsNullOrEmpty(request.sessionId))
                    {
                        messageWriter.WriteStartAttribute("sessionId");
                        messageWriter.WriteString(request.sessionId);
                        messageWriter.WriteEndAttribute();
                    }
                    if (request.hold != null)
                    {
                        messageWriter.WriteStartElement("hold");
                        messageWriter.WriteEndElement();
                    }
                    else if (request.retrieve != null)
                    {
                        messageWriter.WriteStartElement("retrieve");
                        messageWriter.WriteEndElement();
                    }
                    else if (request.bargein != null)
                    {
                        messageWriter.WriteStartElement("bargein");
                        messageWriter.WriteEndElement();
                    }
                    else if (request.whisper != null)
                    {
                        messageWriter.WriteStartElement("whisper");
                        messageWriter.WriteStartElement("uri");
                        messageWriter.WriteString(request.whisper.uri);
                        messageWriter.WriteEndElement();

                        messageWriter.WriteEndElement();
                    }
                    else if (request.escalate != null)
                    {
                        messageWriter.WriteStartElement("escalate");
                        messageWriter.WriteStartElement("agentSkills");

                        foreach (agentSkillType skill in request.escalate.agentSkills)
                        {
                            messageWriter.WriteStartElement("agentSkill");

                            messageWriter.WriteStartAttribute("name");
                            messageWriter.WriteString(skill.name);
                            messageWriter.WriteEndAttribute();

                            messageWriter.WriteString(skill.Value);

                            messageWriter.WriteEndElement();
                        }

                        messageWriter.WriteEndElement();
                        messageWriter.WriteEndElement();
                    }
                    else if (request.startmonitoringsession != null)
                    {
                        messageWriter.WriteStartElement("start-monitoring-session");

                        messageWriter.WriteStartAttribute("sessionId");
                        messageWriter.WriteString(request.startmonitoringsession.sessionId);
                        messageWriter.WriteEndAttribute();

                        messageWriter.WriteStartElement("uri");
                        messageWriter.WriteString(request.startmonitoringsession.uri);
                        messageWriter.WriteEndElement();

                        messageWriter.WriteEndElement();
                    }
                    else if (request.terminatemonitoringsession != null)
                    {
                        messageWriter.WriteStartElement("terminate-monitoring-session");

                        messageWriter.WriteStartAttribute("sessionId");
                        messageWriter.WriteString(request.terminatemonitoringsession.sessionId);
                        messageWriter.WriteEndAttribute();

                        messageWriter.WriteEndElement();
                    }

                    messageWriter.WriteEndElement(); //end request

                    messageWriter.Close();

                    serializedRequest = sw.ToString();
                }
            }

            return serializedRequest;
        }

        public static responseType ParseResponse(string responseData)
        {
            responseType response = new responseType();

            if (!string.IsNullOrEmpty(responseData))
            {
                XElement element = XElement.Parse(responseData);

                if (element.Attribute("requestId") != null)
                {
                    response.requestId = Convert.ToUInt32(element.Attribute("requestId").Value);
                }

                if (element.Attribute("result") != null)
                {
                    response.result = element.Attribute("result").Value;
                }

                if (element.Attribute("sessionId") != null)
                {
                    response.sessionId = element.Attribute("sessionId").Value;
                }
            }


            return response;
        }

        public static notificationType ParseNotification(string notificationData)
        {
            notificationType notification = new notificationType();

            if (!string.IsNullOrEmpty(notificationData))
            {
                XElement element = XElement.Parse(notificationData);

                if (element.Attribute("sessionId") != null)
                {
                    notification.sessionId = element.Attribute("sessionId").Value;
                }

                List<participantType> participantInfos = new List<participantType>();
                List<string> participantsRemoved = new List<string>();
                List<agentType> agentInfos = new List<agentType>();
                List<string> agentsRemoved = new List<string>();

                if (element.Element("participant-infos") != null)
                {
                    foreach (XElement participantInfo in element.Element("participant-infos").Elements("participant-info"))
                    {
                        participantType notificationParticipantinfo = new participantType();
                        notificationParticipantinfo.uri = participantInfo.Element("uri") == null ? String.Empty : participantInfo.Element("uri").Value;
                        notificationParticipantinfo.displayname = participantInfo.Element("displayname") == null ? "Anonymous" : participantInfo.Element("displayname").Value;
                        notificationParticipantinfo.iscustomer = participantInfo.Element("iscustomer") == null ? false : Convert.ToBoolean(participantInfo.Element("iscustomer").Value);

                        List<string> mediaTypes = new List<string>();
                        if (participantInfo.Element("media-types") != null)
                        {
                            foreach (XElement mediaType in participantInfo.Element("media-types").Elements("media-type"))
                            {
                                if (mediaType != null)
                                {
                                    mediaTypes.Add(mediaType.Value);
                                }
                            }
                        }
                        notificationParticipantinfo.mediatypes = mediaTypes.ToArray();
                        participantInfos.Add(notificationParticipantinfo);
                    }
                }

                if (element.Element("partcicipants-removed") != null)
                {
                    foreach (XElement removedParticipant in element.Element("partcicipants-removed").Elements("uri"))
                    {
                        if (removedParticipant != null)
                        {
                            participantsRemoved.Add(removedParticipant.Value);
                        }
                    }
                }

                if (element.Element("agent-infos") != null)
                {
                    foreach (XElement agentInfo in element.Element("agent-infos").Elements("agent-info"))
                    {
                        agentType notificationAgentinfo = new agentType();
                        notificationAgentinfo.uri = agentInfo.Element("uri") == null ? String.Empty : agentInfo.Element("uri").Value;
                        notificationAgentinfo.displayname = agentInfo.Element("displayname") == null ? String.Empty : agentInfo.Element("displayname").Value;
                        notificationAgentinfo.status = agentInfo.Element("status") == null ? String.Empty : agentInfo.Element("status").Value;
                        notificationAgentinfo.statuschangedtime = agentInfo.Element("status-changed-time") == null ? String.Empty : agentInfo.Element("status-changed-time").Value;

                        List<string> mediaTypes = new List<string>();
                        if (agentInfo.Element("media-types") != null)
                        {
                            foreach (XElement mediaType in agentInfo.Element("media-types").Elements("media-type"))
                            {
                                if(mediaType!=null)
                                {
                                    mediaTypes.Add(mediaType.Value);
                                }
                            }
                        }

                        notificationAgentinfo.mediatypes = mediaTypes.ToArray();
                        agentInfos.Add(notificationAgentinfo);
                    }
                }

                if (element.Element("agents-removed") != null)
                {
                    foreach (XElement removedAgent in element.Element("agents-removed").Elements("uri"))
                    {
                        if(removedAgent!=null)
                        {
                            agentsRemoved.Add(removedAgent.Value);
                        }
                    }
                }

                notification.agentinfos = agentInfos.ToArray();
                notification.agentsremoved = agentsRemoved.ToArray();
                notification.participantinfos = participantInfos.ToArray();
                notification.partcicipantsremoved = participantsRemoved.ToArray();
            }

            return notification;

        }

        #endregion
    }
}