/*=====================================================================
  File:      ReverseLookUp.cs

  Summary:   Implements a simple scheme to find the identity of the owner of a specific phone number.
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Microsoft.Rtc.Signaling;
using System.Collections.ObjectModel;
using Microsoft.Rtc.Collaboration.Samples.Utilities;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public abstract class ReverseNumberLookup : ComponentBase
    {
        public ReverseNumberLookup(AppPlatform platform)
            : base(platform)
        {
        }

        #region Pubic interface

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Required processing is involved.")]
        public static ReverseNumberLookup GetLookupInstance(AppPlatform platform)
        {
            return new RnlXmlFileImplementation(platform);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification="This is a sip uri.")]
        public IAsyncResult BeginLookup(string telUri, AsyncCallback userCallback, object state)
        {
            VerifyTelUri(telUri);
            
            return this.BeginLookupCore(telUri, userCallback, state);
        }

        public ReverseNumberLookupResult EndLookup(IAsyncResult result)
        {
            return this.EndLookupCore(result);
        }

        public virtual Collection<string> FindPhoneNumbers(string sipUri)
        {
            return new Collection<string>(); // Empty Collection. Subclass should implement real one.
        }

        /// <summary>
        /// Add a new entry.
        /// </summary>
        /// <param name="telUri">The tel uri.</param>
        /// <param name="sipUri">The matching sip uri.</param>
        /// <returns>True if added successfully. False, otherwise.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "This is a sip uri.")]
        public virtual bool AddEntry(string telUri, string sipUri)
        {
            return false;
        }

        /// <summary>
        /// Remove an entry.
        /// </summary>
        /// <param name="telUri">The tel uri entry to remove.</param>
        /// <returns>True if removed.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "This is a sip uri.")]
        public virtual bool RemoveEntry(string telUri)
        {
            return false;
        }

        /// <summary>
        /// Remove an entry.
        /// </summary>
        /// <param name="telUri">The tel uri entry to remove.</param>
        /// <param name="sipUri">Matching sip uri, if known. Can be null.</param>
        /// <returns>True if removed.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "This is a sip uri.")]
        public virtual bool RemoveEntry(string telUri, string sipUri)
        {
            return false;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#")]
        protected abstract IAsyncResult BeginLookupCore(string telUri, AsyncCallback userCallback, object state);

        protected abstract ReverseNumberLookupResult EndLookupCore(IAsyncResult result);

        #endregion

        #region Private implementation

        private static void VerifyTelUri(string telUri)
        {
            if (string.IsNullOrEmpty(telUri))
            {
                throw new ArgumentNullException("telUri", "Empty or null string");
            }

            //TODO:Do more verification here to ensure that the telUri is in the correct format.
        }        

        #endregion

        #region RnlFileImplementation

        private class RnlXmlFileImplementation : ReverseNumberLookup
        {
            #region Private fields

            private readonly string m_filePath;
            private readonly Dictionary<string, string> m_directory;
            private bool m_isModified;
            private TimerItem m_fileSaveTimer; // Timer to save XML file periodically if it was modified.
            private TimeSpan m_fileSaveTimeSpan = new TimeSpan(0, 1, 0); // Every 1 hr, try to save XML file.
            #endregion

            #region Public interface

            public RnlXmlFileImplementation(AppPlatform platform)
                : base(platform)
            {
                m_filePath = ApplicationConfiguration.RnlFile;
                m_directory = new Dictionary<string, string>();
                m_fileSaveTimer = new TimerItem(platform.TimerWheel, m_fileSaveTimeSpan);
                m_fileSaveTimer.Expired += FileSaveTimerExpired;
            }

            void FileSaveTimerExpired(object sender, EventArgs e)
            {
                this.SaveXmlFile();
                m_fileSaveTimer.Reset();
            }

            public override Collection<string> FindPhoneNumbers(string sipUri)
            {
                Collection<string> list = new Collection<string>();
                RealTimeAddress sipUriAddress = null;
                try
                {
                    sipUriAddress = new RealTimeAddress(sipUri);
                    if (sipUriAddress.IsPhone) // Should not be phone uri
                    {
                        return list;
                    }
                }
                catch (ArgumentException)
                {
                    return list;
                }
                lock (this.SyncRoot)
                {
                    foreach (string key in m_directory.Keys)
                    {
                        RealTimeAddress valueAddress = new RealTimeAddress(m_directory[key]);
                        if (valueAddress == sipUriAddress)
                        {
                            list.Add(key);
                        }                        
                    }
                }
                return list;
            }

            /// <summary>
            /// Add a new entry.
            /// </summary>
            /// <param name="telUri">The tel uri.</param>
            /// <param name="sipUri">The matching sip uri.</param>
            /// <returns>True if added successfully. False, otherwise.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "This is a sip uri.")]
            public override bool AddEntry(string telUri, string sipUri)
            {
                bool wasAdded = false;
                try
                {
                    if (!telUri.StartsWith("tel"))
                    {
                        telUri = "tel:" + telUri;
                    }
                    RealTimeAddress telUriAddress = new RealTimeAddress(telUri);
                    if (!telUriAddress.IsPhone) // Should be phone uri.
                    {
                        return wasAdded;
                    }
                    RealTimeAddress sipUriAddress = new RealTimeAddress(sipUri);
                    if (sipUriAddress.IsPhone) // Should not be phone uri
                    {
                        return wasAdded; 
                    }
                }
                catch (ArgumentException)
                {
                    return wasAdded;
                }

                lock (this.SyncRoot)
                {
                    if (!m_directory.ContainsKey(telUri))
                    {
                        m_directory.Add(telUri, sipUri);
                        wasAdded = true;
                        m_isModified = true;
                    }
                }
                return wasAdded;
            }

            public override bool RemoveEntry(string telUri)
            {
                return this.RemoveEntry(telUri, null);
            }

            /// <summary>
            /// Remove an entry.
            /// </summary>
            /// <param name="telUri">The tel uri entry to remove.</param>
            /// <returns>True if removed.</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "This is a sip uri.")]
            public override bool RemoveEntry(string telUri, string sipUri)
            {
                bool removed = false;
                lock (this.SyncRoot)
                {
                    if (!telUri.StartsWith("tel"))
                    {
                        telUri = "tel:" + telUri;
                    }
                    if (m_directory.ContainsKey(telUri))
                    {
                        bool shouldRemove = true;
                        if (!String.IsNullOrEmpty(sipUri))
                        {
                            string oldSipUri = m_directory[telUri];

                            try
                            {
                                RealTimeAddress oldSipUriAddress = new RealTimeAddress(oldSipUri);
                                RealTimeAddress telUriAddress = new RealTimeAddress(telUri);
                                RealTimeAddress sipUriAddress = new RealTimeAddress(sipUri);
                                if (oldSipUriAddress != sipUriAddress)
                                {
                                    shouldRemove = false; // Does not match. One can only remove their own number.
                                }
                            }
                            catch (ArgumentException)
                            {
                                shouldRemove = false;
                            }
                        }
                        if (shouldRemove)
                        {
                            m_directory.Remove(telUri);
                            removed = true;
                            m_isModified = true;
                        }
                    }
                }
                return removed;
            }

            protected override IAsyncResult BeginLookupCore(string telUri, AsyncCallback userCallback, object state)
            {
                ReverseNumberLookupResult result = new ReverseNumberLookupResult();

                lock (this.SyncRoot)
                {                    
                    if (this.IsTerminatingTerminated)
                    {
                        throw new InvalidOperationException("Cannot perform the operation as the component is terminating");
                    }
                    
                    if (m_directory.ContainsKey(telUri))
                    {
                        result.WasNumberFound = true;
                        result.Uri = m_directory[telUri];
                    }
                    else
                    {
                        result.WasNumberFound = false;
                        result.Uri = string.Empty;
                    }
                }

                var asyncResult = new AsyncResult<ReverseNumberLookupResult>(userCallback, state);
                asyncResult.SetAsCompleted(result, true);
                return asyncResult;
            }

            protected override ReverseNumberLookupResult EndLookupCore(IAsyncResult result)
            {
                return ((AsyncResult<ReverseNumberLookupResult>)result).EndInvoke();
            }

            protected override void StartupCore()
            {
                LookupFailureException ex = null;

                lock (this.SyncRoot)
                {
                    if (this.IsTerminatingTerminated)
                    {
                        return;
                    }

                    var root = XElement.Load(m_filePath);
                    if (root != null)
                    {
                        var entries = root.Descendants("entry").ToList();
                        if (entries != null)
                        {
                            entries.ForEach(item => m_directory.Add(item.Attribute("number").Value, item.Element("Uri").Value));
                        }
                        else
                        {
                            ex = new LookupFailureException("Failed to look up the number. See inner exception for details."); ;
                        }
                    }
                    else
                    {
                        ex = new LookupFailureException("Failed to look up the number. See inner exception for details."); ;
                    }
                    m_fileSaveTimer.Start();
                }
                this.CompleteStartup (ex);

            }

            protected override void ShutdownCore()
            {
                lock (this.SyncRoot)
                {
                    this.SaveXmlFile();
                    m_directory.Clear();
                    m_fileSaveTimer.Stop();
                }

                this.CompleteShutdown();
            }

            private void SaveXmlFile()
            {
                lock (this.SyncRoot)
                {
                    if (m_isModified)
                    {
                        // Save m_directory into the file to persist changes.
                        XElement root = new XElement("root");
                        foreach (string key in m_directory.Keys)
                        {
                            root.Add(new XElement("entry", new XAttribute("number", key), new XElement("Uri", m_directory[key])));
                        }
                        root.Save(m_filePath);
                        Logger.Log(Logger.LogLevel.Verbose, "RNL file Saved.");
                        m_isModified = false;
                    }
                }
            }

            #endregion
        }

        #endregion
    }

    public class ReverseNumberLookupResult
    {
        #region Public interface

        public bool WasNumberFound
        {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "This is a SIP uri.")]
        public string Uri
        {
            get;
            set;
        }

        #endregion
    }

    [Serializable]
    public class LookupFailureException : Exception
    {
        public LookupFailureException():base()
        {
        }

        public LookupFailureException(string message)
            : base(message)
        {
        }

        public LookupFailureException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected LookupFailureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

}
