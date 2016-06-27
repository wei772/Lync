/*=====================================================================
  File:      AgentDashboardChannel.cs

  Summary:   The model for agent dashboard view. 

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
using System.Linq;
using System.Xml.Linq;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Samples.ContactCenterExtension.ViewModels;

namespace Microsoft.Lync.Samples.ContactCenterExtension.Models
{
    /// <summary>
    /// The model for agent dashboard.
    /// </summary>
    public class AgentDashboardChannel
    {
        #region Fields

        private List<skillType> _skills;
        private readonly RequestProcessor _requestProcessor;

        #endregion

        #region Properties

        public productType ProductInfo { get; private set; }
        public Conversation Conversation { get; private set; }
        public string ApplicationId { get; private set; }
        public Collection<skillType> Skills
        {
            get { return new Collection<skillType>(_skills); }
        }

        #endregion

        #region Constructors

        public AgentDashboardChannel(string guid)
        {
            Conversation = LyncClient.GetHostingConversation() as Conversation;

            ApplicationId = guid;
            _requestProcessor = new RequestProcessor(null);

            Initialize();

            if (Conversation != null)
            {
                Conversation.ContextDataReceived += ConversationContextDataReceived;
            }
        }

        #endregion

        #region Public Methods

        public IAsyncResult BeginHold(AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.Hold);

            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, Conversation, ApplicationId,
                                              userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public void EndHold(IAsyncResult result)
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

        public void EndRetrieve(IAsyncResult result)
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

        public IAsyncResult BeginEscalate(IEnumerable<agentSkillType> skills,
                                          AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.Escalate);

            request.escalate = new escalateType();
            request.escalate.agentSkills = skills.ToArray();

            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, Conversation, ApplicationId,
                                              userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public IAsyncResult BeginRetrieve(AsyncCallback userCallback, object state)
        {
            requestType request = _requestProcessor.CreateRequest(ContextChannelRequestType.Retrieve);

            ProcessRequestAsyncResult requestAsyncResult =
                new ProcessRequestAsyncResult(request, _requestProcessor, Conversation, ApplicationId,
                                              userCallback, state);

            _requestProcessor.AddPendingRequest(request, requestAsyncResult);

            requestAsyncResult.Process();


            return requestAsyncResult;
        }

        public void EndEscalate(IAsyncResult result)
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

        #region Private Methods

        private void Initialize()
        {
            string appData = Conversation.GetApplicationData(ApplicationId);

            productType productInfo = new productType();

            XElement element = XElement.Parse(appData);
            XElement productElement = element.Element("product");

            if (productElement != null)
            {
                if (productElement.Element("productDescription") != null)
                {
                    productInfo.productDescription = productElement.Element("productDescription").Value;
                }

                if (productElement.Element("productImage") != null)
                {
                    productInfo.productImage = productElement.Element("productImage").Value;
                }

                if (productElement.Element("productGuid") != null)
                {
                    productInfo.productGuid = productElement.Element("productGuid").Value;
                }

                if (productElement.Element("productPrice") != null)
                {
                    productInfo.productPrice = productElement.Element("productPrice").Value;
                }

                if (productElement.Element("productTitle") != null)
                {
                    productInfo.productTitle = productElement.Element("productTitle").Value;
                }
            }

            ProductInfo = productInfo;

            _skills = new List<skillType>();

            XElement skillsElement = element.Element("skills");

            foreach (XElement skillElement in skillsElement.Elements("skill"))
            {
                skillType skill = new skillType();

                if (skillElement.Attribute("name") != null)
                {
                    skill.name = skillElement.Attribute("name").Value;
                }

                List<string> skillValues = new List<string>();

                if (skillElement.Element("skillValues") != null)
                {
                    foreach (XElement skillValueElement in skillElement.Element("skillValues").Elements("skillValue"))
                    {
                        skillValues.Add(skillValueElement.Value);
                    }
                }

                skill.skillValues = skillValues.ToArray();
                _skills.Add(skill);
            }

        }

        void ConversationContextDataReceived(object sender, ContextEventArgs e)
        {
            if (e.ContextDataType.Equals(WireHelpers.Response, StringComparison.OrdinalIgnoreCase))
            {
                ProcessResponse(WireHelpers.ParseResponse(e.ContextData));
            }
        }

        private void ProcessResponse(responseType response)
        {
            _requestProcessor.ProcessResponse(response);
        }

        #endregion

    }
}
