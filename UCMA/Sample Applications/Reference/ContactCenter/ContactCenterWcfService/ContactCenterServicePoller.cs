
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.AsyncResults;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService
{
    /// <summary>
    /// Represents service poller class to keep polling the contact center service for update queue to uri mapping.
    /// </summary>
    internal class ContactCenterServicePoller
    {
        #region private variables

        private readonly TimerWheel m_timerWheel;

        private static readonly TimeSpan DefaultPollingTimeSpan = new TimeSpan(0/*hours*/, 1/*minutes*/, 0/*seconds*/);

        /// <summary>
        /// Local endpoint.
        /// </summary>
        private readonly LocalEndpoint m_endpoint;

        /// <summary>
        /// Contact center uri.
        /// </summary>
        private readonly string m_contactCenterUri;

        /// <summary>
        /// Sync root object.
        /// </summary>
        private readonly object m_syncRoot = new object();

        /// <summary>
        /// Timer item.
        /// </summary>
        private TimerItem m_timerItem;

        /// <summary>
        /// Latest contact center information.
        /// </summary>
        private ContactCenterInformation m_contactCenterInformation;
        #endregion

        #region constructor
        /// <summary>
        /// Constructor to create the poller.
        /// </summary>
        /// <param name="endpoint">Endpoint. Cannot be null.</param>
        /// <param name="contactCenterUri">Contact center uri. Cannot be null or empty.</param>
        /// <param name="timerWheel">Timerwheel</param>
        internal ContactCenterServicePoller(LocalEndpoint endpoint, string contactCenterUri, TimerWheel timerWheel)
        {
            Debug.Assert(null != endpoint, "Endpoint is null");
            Debug.Assert(!String.IsNullOrEmpty(contactCenterUri), "Contact center uri is null or empty");
            m_endpoint = endpoint;
            m_contactCenterUri = contactCenterUri;
            m_timerWheel = timerWheel;
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
        /// Gets the contact center uri.
        /// </summary>
        private string ContactCenterUri
        {
            get { return m_contactCenterUri; }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets the latest contact center information. Can return null if contact center information is not available.
        /// </summary>
        internal ContactCenterInformation ContactCenterInformation
        {
            get
            {
                return m_contactCenterInformation; 
            }
        }
        #endregion

        #region internal methods

        /// <summary>
        /// Get uri from queue name.
        /// </summary>
        /// <param name="queueName">Queue name.</param>
        /// <returns>Uri value from queue name.</returns>
        internal string GetUriFromQueueName(string queueName)
        {
            string retVal = queueName;
            ContactCenterInformation ccInfo = this.ContactCenterInformation;
            if (!string.IsNullOrEmpty(queueName) && ccInfo != null)
            {
                string uriValue = ccInfo.GetUriFromQueueName(queueName);
                //If we cannot find the uri valuejust return the queue name.
                if (!String.IsNullOrEmpty(uriValue))
                {
                    retVal = uriValue;
                }
            }
            return retVal;
        }

        /// <summary>
        /// Starts the poller.
        /// </summary>
        internal void Start()
        {
            lock (m_syncRoot)
            {
                this.StartRetrievingNewData();
                if (m_timerItem == null)
                {
                    m_timerItem = new TimerItem(m_timerWheel, ContactCenterServicePoller.DefaultPollingTimeSpan);
                    m_timerItem.Expired += this.TimerItem_Expired;
                    m_timerItem.Start();
                }
                else if(!m_timerItem.IsStarted)
                {
                    m_timerItem.Start();
                }
            }
        }

        /// <summary>
        /// Stops the poller.
        /// </summary>
        internal void Stop()
        {
            lock (m_syncRoot)
            {
                if (m_timerItem != null)
                {
                    m_timerItem.Stop();
                    m_timerItem.Expired -= this.TimerItem_Expired;
                    m_timerItem = null;
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Timer expired callback.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void TimerItem_Expired(object sender, EventArgs e)
        {
            //Request for new data.
            lock (m_syncRoot)
            {
                this.StartRetrievingNewData();
                m_timerItem.Reset();
            }
        }

        /// <summary>
        /// Method to start the work of retrieving new data.
        /// </summary>
        private void StartRetrievingNewData()
        {
            lock (m_syncRoot)
            {
                ContactCenterDiscoveryAsyncResult asyncResult = new ContactCenterDiscoveryAsyncResult(this.LocalEndpoint, this.ContactCenterUri, this.RetrieveCompleted, null /*state*/);
                asyncResult.Process();
            }
        }

        /// <summary>
        /// Callback method after data retrieval is complete.
        /// </summary>
        /// <param name="asyncResult">Async result refrence.</param>
        private void RetrieveCompleted(IAsyncResult asyncResult)
        {
            lock (m_syncRoot)
            {
                ContactCenterDiscoveryAsyncResult contactCenterDiscoveryAsyncResult = asyncResult as ContactCenterDiscoveryAsyncResult;
                if (contactCenterDiscoveryAsyncResult.Exception == null)
                {
                    //Update only in successful cases.
                    m_contactCenterInformation = contactCenterDiscoveryAsyncResult.EndInvoke();
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// Encapsulates all contact center information related details.
    /// </summary>
    internal class ContactCenterInformation
    {
        #region private variables

        /// <summary>
        /// Dictionary which has queue to uri mapping.
        /// </summary>
        private readonly IDictionary<string, string> m_queueUriMapping;
        #endregion

        #region constructor

        /// <summary>
        /// Creates new contact center information with a dictionary containing mapping of queue name to uri.
        /// </summary>
        /// <param name="queueUriMapping">Queue uri mapping dictionary.</param>
        internal ContactCenterInformation(IDictionary<string, string> queueUriMapping) 
        {
            Debug.Assert(null != queueUriMapping, "Queue uri mapping is null");
            //Take a copy.
            m_queueUriMapping = new Dictionary<string, string>(queueUriMapping, StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region private properties
        /// <summary>
        /// Queue uri mapping.
        /// </summary>
        private IDictionary<string, string> QueueUriMapping
        {
            get { return m_queueUriMapping; }
        }
        #endregion

        #region internal methods

        /// <summary>
        /// Gets uri from queue name.
        /// </summary>
        /// <param name="queueName">Queue name. Cannot be null or empty.</param>
        /// <returns>Uri name if available. Else null.</returns>
        internal string GetUriFromQueueName(string queueName)
        {
            string uriValue = null;
            if (!String.IsNullOrEmpty(queueName))
            {
                if (!this.QueueUriMapping.TryGetValue(queueName, out uriValue))
                {
                    uriValue = null;
                }
            }
            return uriValue;
        }

        /// <summary>
        /// Gets all available queue names.
        /// </summary>
        /// <returns>List of all available queue names.</returns>
        internal List<string> GetAllAvailableQueueNames()
        {
            return new List<string>(this.QueueUriMapping.Keys);
        }

        #endregion
    }

}
