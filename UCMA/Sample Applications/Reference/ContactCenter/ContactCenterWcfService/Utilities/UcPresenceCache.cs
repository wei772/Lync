
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;
using Microsoft.Rtc.Collaboration.Presence;
using System.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    internal class UcPresenceCache
    {
        private Dictionary<string, PresenceInformation> m_presenceSubscriptionsBySipUri = new Dictionary<string, PresenceInformation>(StringComparer.OrdinalIgnoreCase);
        private object m_syncObject = new object();
        private Timer m_staleTimer;
        private DateTime StaleTime { get { return DateTime.Now - StaleInterval; } }

        private UcPresenceProvider m_presenceProvider;

        /// <summary>
        /// The interval to run cache cleanup. If any presence subscriptions have not been requested in the last stale interval
        /// they will be removed from the cache
        /// </summary>
        internal TimeSpan StaleInterval { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="applicationEndpoint">App endpoint.</param>
        internal UcPresenceCache(ApplicationEndpoint applicationEndpoint)
        {
            m_presenceProvider = new UcPresenceProvider(applicationEndpoint);
            m_presenceProvider.Start();
            m_presenceProvider.PresenceChanged += this.PresenceProvider_PresenceChanged;
            StaleInterval = TimeSpan.FromHours(2);
            m_staleTimer = new Timer(OnStaleTimerTick, null, StaleInterval, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Clean up cache.
        /// </summary>
        internal void CleanupCache()
        {
            lock (m_syncObject)
            {
                UcPresenceProvider presenceProvider = m_presenceProvider;
                if (presenceProvider != null)
                {
                    presenceProvider.Stop();
                    presenceProvider.PresenceChanged -= this.PresenceProvider_PresenceChanged;
                }
                ///Clear the cache.
                m_presenceSubscriptionsBySipUri.Clear();
            }
        }

        /// <summary>
        /// Get presence.
        /// </summary>
        /// <param name="sipUri"></param>
        /// <returns></returns>
        public PresenceInformation GetPresence(string sipUri)
        {
            lock (m_syncObject)
            {
                if (!m_presenceSubscriptionsBySipUri.ContainsKey(sipUri))
                {
                    m_presenceSubscriptionsBySipUri.Add(sipUri, new PresenceInformation
                    {
                        Availability = (long)PresenceAvailability.None,
                        ContactName = "Unknown",
                        SipUri = sipUri,
                        Status = "Unknown",
                        SubscriptionCreateTime = DateTime.Now,
                        ActivityStatus = "Current wait time is unknown"
                    });
                    m_presenceProvider.SubscribeToPresence(sipUri);
                }
                //update the timestamp whenever the presence is requested
                //this prevents a presence from being cleared out of the cache at the StaleInterval
                m_presenceSubscriptionsBySipUri[sipUri].SubscriptionCreateTime = DateTime.Now;
                return m_presenceSubscriptionsBySipUri[sipUri];
            }
        }

        /// <summary>
        /// Gets presence for a list of uris.
        /// </summary>
        /// <param name="sipUris">List of uris.</param>
        /// <returns></returns>
        public List<PresenceInformation> GetPresence(List<string> sipUris)
        {
            List<PresenceInformation> presences = new List<PresenceInformation>();
            lock (m_syncObject)
            {
                foreach (string sipUri in sipUris)
                {
                    presences.Add(GetPresence(sipUri));
                }
            }
            return presences;
        }

        #region Manage Presence Changes
        private void PresenceProvider_PresenceChanged(object sender, PresenceChangedEventArgs e)
        {
            lock (m_syncObject)
            {
                foreach (PresenceInformation presence in e.PresenceSubscriptions)
                {
                    if (m_presenceSubscriptionsBySipUri.ContainsKey(presence.SipUri))
                    {
                        UpdatePresenceSubscriptionWithChanges(presence);
                    }
                    else //received a presence changed event for an expired subscription
                    {
                        Helper.Logger.Info("Ignoring presence update for sipUri {0} since there are no active subscriptions for this uri", presence.SipUri);
                    }
                }
            }
        }

        private void UpdatePresenceSubscriptionWithChanges(PresenceInformation presence)
        {
            if (presence.Availability != PresenceInformation.NoChangeInAvailability)
            {
                m_presenceSubscriptionsBySipUri[presence.SipUri].Availability = presence.Availability;
            }
            if (presence.ContactName != PresenceInformation.NoChange)
            {
                m_presenceSubscriptionsBySipUri[presence.SipUri].ContactName = presence.ContactName;
            }
            if (presence.Status != PresenceInformation.NoChange)
            {
                m_presenceSubscriptionsBySipUri[presence.SipUri].Status = presence.Status;
            }
            if (presence.ActivityStatus != PresenceInformation.NoChange)
            {
                m_presenceSubscriptionsBySipUri[presence.SipUri].ActivityStatus = presence.ActivityStatus;
            }
        }
        #endregion

        #region Cache Cleanup
        /// <summary>
        /// Removes stale subscriptions.
        /// </summary>
        private void RemoveStaleSubscriptions()
        {
            lock (m_syncObject)
            {
                DateTime staleTime = this.StaleTime;
                var staleKvps = m_presenceSubscriptionsBySipUri.Where(kvp => kvp.Value.SubscriptionCreateTime <= this.StaleTime).ToList();
                foreach (var staleKvp in staleKvps)
                {
                    string sipUri = staleKvp.Key;
                    Helper.Logger.Info("Removing stale subscription for sip uri {0}, last requested at {1}", sipUri, staleKvp.Value.SubscriptionCreateTime);
                    m_presenceProvider.UnsubscribeToPresence(sipUri);
                    m_presenceSubscriptionsBySipUri.Remove(sipUri);
                }
            }
        }

        /// <summary>
        /// Timer tick handler.
        /// </summary>
        /// <param name="state">State.</param>
        private void OnStaleTimerTick(object state)
        {
            //stop the stale timer
            m_staleTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
            this.RemoveStaleSubscriptions();
            m_staleTimer.Change(this.StaleInterval, TimeSpan.FromMilliseconds(-1));
        }
        #endregion



    }
}
