/********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using System.Diagnostics;
using Microsoft.Rtc.Signaling;
using System.Net.Mime;
using System.Xml.Serialization;
using System.Xml;


namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults
{
    /// <summary>
    /// Represents contact center discovery async result.
    /// </summary>
    internal class ContactCenterDiscoveryAsyncResult : AsyncResultWithProcess<ContactCenterInformation>
    {

        #region private consts
        /// <summary>
        /// Content type of the service request.
        /// </summary>
        private const string DiscoveryContentType = "application/ContactCenterDiscovery+xml";
        #endregion

        #region private variables

        /// <summary>
        /// Local endpoint.
        /// </summary>
        private readonly LocalEndpoint m_endpoint;

        /// <summary>
        /// Contact center uri.
        /// </summary>
        private readonly string m_targetUri;
        #endregion

        #region constructor

        /// <summary>
        /// Creates new Contact center discovery async result.
        /// </summary>
        /// <param name="localEndpoint"></param>
        /// <param name="targetUri"></param>
        /// <param name="localContactHeaderValue"></param>
        internal ContactCenterDiscoveryAsyncResult(LocalEndpoint localEndpoint, string targetUri, AsyncCallback userCallback, object state)
            : base(userCallback, state)
        {
            Debug.Assert(null != localEndpoint, "Local endpoint is null");
            Debug.Assert(!String.IsNullOrEmpty(targetUri), "Target uri is null or empty");

            m_endpoint = localEndpoint;
            m_targetUri = targetUri;
        }
        #endregion

        #region private properties

        /// <summary>
        /// Gets the local endpoint.
        /// </summary>
        private LocalEndpoint LocalEndpoint
        {
            get { return m_endpoint; }
        }

        /// <summary>
        /// Gets the target uri - contact center default endpoint gruu.
        /// </summary>
        private string TargetUri
        {
            get { return m_targetUri; }
        }
        #endregion

        #region overrridden methods

        /// <summary>
        /// Override process method.
        /// </summary>
        public override void Process()
        {
            bool unhandledExceptionDetected = true;
            Exception exceptionCaught = null;

            try
            {
                RealTimeEndpoint innerEndpoint = this.LocalEndpoint.InnerEndpoint;
                SendMessageOptions options = new SendMessageOptions();
                options.ContentDescription = ContactCenterDiscoveryAsyncResult.GetDiscoveryRequestContentDescription();
                RealTimeAddress targetAddress = new RealTimeAddress(this.TargetUri);
                innerEndpoint.BeginSendMessage(MessageType.Service, targetAddress, options, this.ServiceRequestCompleted, innerEndpoint/*state*/);
                unhandledExceptionDetected = false;
            }
            catch (ArgumentException ae)
            {
                Helper.Logger.Error("Exception = {0}", EventLogger.ToString(ae));
                exceptionCaught = ae;
                unhandledExceptionDetected = false;
            }
            catch (InvalidOperationException ioe)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(ioe));
                exceptionCaught = ioe;
                unhandledExceptionDetected = false;
            }
            catch (RealTimeException rte)
            {
                Helper.Logger.Info("Exception = {0}", EventLogger.ToString(rte));
                exceptionCaught = rte;
                unhandledExceptionDetected = false;
            }
            finally
            {
                if (unhandledExceptionDetected)
                {
                    exceptionCaught = new Exception("Unhandled exception");
                    Helper.Logger.Info("Exception = {0}", EventLogger.ToString(exceptionCaught));
                }

                if(exceptionCaught != null)
                {
                    this.Complete(exceptionCaught);
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Callback method for service completed.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private void ServiceRequestCompleted(IAsyncResult asyncResult)
        {
            RealTimeEndpoint innerEndpoint = asyncResult.AsyncState as RealTimeEndpoint;
            Debug.Assert(null != innerEndpoint, "Inner endpoint is null");
            bool unhandledExceptionDetected = true;
            Exception exceptionCaught = null;
            ContactCenterInformation result = null;

            try
            {
                SipResponseData serviceResponse = innerEndpoint.EndSendMessage(asyncResult);
                byte[] responseBody = serviceResponse.GetMessageBody();

                if (responseBody != null)
                {
                    result = ContactCenterDiscoveryAsyncResult.DeserializeResponseData(responseBody);
                }

                if (result == null)
                {
                    //Deserialziation failed.
                    exceptionCaught = new XmlException("Deserialization of queue uri mapping failed");
                }
                unhandledExceptionDetected = false;
            }
            catch (XmlException xe)
            {
                exceptionCaught = xe;
                unhandledExceptionDetected = false;
            }
            catch (ArgumentException ae)
            {
                exceptionCaught = ae;
                unhandledExceptionDetected = false;
            }
            catch (RealTimeException rte)
            {
                exceptionCaught = rte;
                unhandledExceptionDetected = false;
            }
            finally
            {
                if (unhandledExceptionDetected)
                {
                    exceptionCaught = new Exception("Unhandled exception");
                    Helper.Logger.Info("Exception = {0}", EventLogger.ToString(exceptionCaught));
                }

                if (exceptionCaught != null)
                {
                    Helper.Logger.Error("Exception = {0}", EventLogger.ToString(exceptionCaught));
                    this.Complete(exceptionCaught);
                }
                else
                {
                    Debug.Assert(null != result, "If no exception occured, we expect a valid result");
                    this.Complete(result);
                }
            }
        }

        /// <summary>
        /// Method to create discovery request content description.
        /// </summary>
        /// <returns></returns>
        private static ContentDescription GetDiscoveryRequestContentDescription()
        {
            ContentType contentType = new ContentType(ContactCenterDiscoveryAsyncResult.DiscoveryContentType);
            ContentDescription contentDescription = new ContentDescription(contentType, string.Empty /*body*/);
            return contentDescription;
        }

        /// <summary>
        /// Method to parse service response and create contact center information.
        /// </summary>
        /// <returns>Contact center information.</returns>
        /// <exception cref="XmlException">Thrown when deserialization fails.</exception>
        private static ContactCenterInformation DeserializeResponseData(byte[] responseBytes)
        {
            ContactCenterInformation contactCenterInformation = null;
            if (responseBytes != null && responseBytes.Length > 0)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(queueUriMappingListType));
                queueUriMappingListType deserializedResult = CommonHelper.DeserializeObjectFragment(responseBytes, serializer) as queueUriMappingListType;
                if (deserializedResult != null && deserializedResult.queueUriMapping != null && deserializedResult.queueUriMapping.Length > 0)
                {
                    Dictionary<string, string> mappingData = new Dictionary<string, string>(deserializedResult.queueUriMapping.Length);
                    foreach (queueUriMappingType queueUriMapping in deserializedResult.queueUriMapping)
                    {
                        if (queueUriMapping != null && !String.IsNullOrEmpty(queueUriMapping.queueName) && !String.IsNullOrEmpty(queueUriMapping.uriValue))
                        {
                            mappingData.Add(queueUriMapping.queueName, queueUriMapping.uriValue);
                        }
                    }
                    contactCenterInformation = new ContactCenterInformation(mappingData);
                }
            }
            return contactCenterInformation;
        }
        #endregion
    }
}
