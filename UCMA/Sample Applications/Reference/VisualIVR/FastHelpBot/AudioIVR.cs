/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelpServer
{
    using System;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using System.Threading;
    using Microsoft.Speech.Synthesis;
    using Microsoft.Rtc.Collaboration;
    using System.Collections.Generic;
    using FastHelpCore;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Globalization;
    using Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml;
    using Microsoft.Speech.VoiceXml;
    using Microsoft.Speech.VoiceXml.Common;
    using System.IO;
    using Microsoft.Rtc.Signaling;
    using FastHelp.Logging;

    /// <summary>
    /// IVR to handle incoming audio calls.
    /// </summary>
    public class AudioIVR : IDisposable
    {
        private AudioVideoFlow audioVideoFlow;
        private AudioVideoCall audioVideoCall;

        private AutoResetEvent waitForMenuInput = new AutoResetEvent(false);
        private AutoResetEvent waitForAudioVideoFlowStateChangedToActiveCompleted = new AutoResetEvent(false);
        private AutoResetEvent waitForAudioVideoCallEstablished = new AutoResetEvent(false);
        private AutoResetEvent[] waitToProcessCall;

        private XmlParser parser;
        private int currentLevel = -1;
        private List<string> selectedOptions;
        private string vxmlDirectory;

        private ILogger logger;

        /// <summary>
        /// VXML browser.
        /// </summary>
        private Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml.Browser voiceXmlBrowser;

        public AudioIVR(AudioVideoCall call, XmlParser parser, ILogger log)
        {
            audioVideoCall = call;
            audioVideoCall.AudioVideoFlowConfigurationRequested +=
                                this.AudioVideoCall_FlowConfigurationRequested;
            audioVideoCall.StateChanged +=
                                this.AudioVideoCall_StateChanged;

            this.parser = parser;
            selectedOptions = new List<string>();
            this.logger = log;

            waitToProcessCall = new AutoResetEvent[] {
                waitForAudioVideoFlowStateChangedToActiveCompleted, waitForAudioVideoCallEstablished 
            };

            vxmlDirectory = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Voicexml");

            ThreadPool.QueueUserWorkItem(this.ProcessCall);
        }

        // Handler for the AudioVideoFlowConfigurationRequested event on the call.
        // This event is raised when there is a 
        //flow present to begin media operations with, and that it is no longer null.
        public void AudioVideoCall_FlowConfigurationRequested(object sender,
            AudioVideoFlowConfigurationRequestedEventArgs e)
        {
            audioVideoFlow = e.Flow;
            audioVideoFlow.StateChanged +=
                new EventHandler<MediaFlowStateChangedEventArgs>(AudioVideoFlow_StateChanged);
        }

        private void AudioVideoCall_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            Console.WriteLine("Call Handler Call state changed from " + e.PreviousState + " to " + e.State);
            if (e.State == CallState.Established)
            {
                waitForAudioVideoCallEstablished.Set();
            }
        }

        // Callback that handles when the state of an AudioVideoFlow changes.
        private void AudioVideoFlow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            Console.WriteLine("Flow Handler Call state changed from " + e.PreviousState + " to " + e.State);
            if (e.State == MediaFlowState.Active)
            {
                waitForAudioVideoFlowStateChangedToActiveCompleted.Set();
            }
        }


        private void ProcessCall(object state)
        {
            WaitHandle.WaitAll(waitToProcessCall);

            InitializeVoiceXmlBrowser();

            PlayMenu("mainmenu");

            waitForMenuInput.WaitOne();

            // Calling part here
            voiceXmlBrowser.Dispose();
            voiceXmlBrowser = null;

            audioVideoCall.StateChanged -= this.AudioVideoCall_StateChanged;

            if (selectedOptions.Count >= 2)
            {
                if (!string.IsNullOrEmpty(selectedOptions[0]) && !string.IsNullOrEmpty(selectedOptions[1]))
                {
                    string helpdeskExtension = parser.HelpdeskNumber(selectedOptions[0], selectedOptions[1]);
                    if (!string.IsNullOrEmpty(helpdeskExtension))
                    {
                        this.logger.Log("Transferring user {0} to helpdesk# {1}", audioVideoCall.OriginalDestinationUri, helpdeskExtension);

                        if (audioVideoCall.State == CallState.Established)
                        {
                            audioVideoCall.BeginTransfer(
                                       helpdeskExtension,
                                       new CallTransferOptions(CallTransferType.Attended),
                                       result =>
                                       {
                                           try
                                           {
                                               audioVideoCall.EndTransfer(result);
                                           }
                                           catch (OperationFailureException ofe)
                                           {
                                               this.logger.Log("The recipient declined or did not answer the call:{0}",
                                                   ofe);
                                           }
                                           catch (RealTimeException rte)
                                           {
                                               this.logger.Log("Error transferring call:{0}", rte);
                                           }
                                       }, null);
                        }
                    }
                    else
                    {
                        this.logger.Log("Unable to find helpdesk number for {0} -> {1}", selectedOptions[0], selectedOptions[1]);
                        TerminateCall();
                    }
                }
                else
                {
                    this.logger.Log("Option Selected are less than 2");
                    TerminateCall();
                }
            }
            else
            {
                this.logger.Log("No user input");
                TerminateCall();
            }
        }

        private void TerminateCall()
        {
            this.logger.Log("Call terminating for user {0}", audioVideoCall.OriginalDestinationUri);

            try
            {
                audioVideoCall.EndTerminate(
                              audioVideoCall.BeginTerminate(result =>
                              {

                              }, null)
                );
            }
            catch { }
        }

        private void InitializeVoiceXmlBrowser()
        {
            voiceXmlBrowser = new Microsoft.Rtc.Collaboration.AudioVideo.VoiceXml.Browser();
            voiceXmlBrowser.Disconnecting
           += new EventHandler<DisconnectingEventArgs>(HandleDisconnecting);
            voiceXmlBrowser.Disconnected
                += new EventHandler<DisconnectedEventArgs>(HandleDisconnected);
            voiceXmlBrowser.SessionCompleted
                += new EventHandler<SessionCompletedEventArgs>(HandleSessionCompleted);

            voiceXmlBrowser.SetAudioVideoCall(audioVideoCall);
        }

        // Handler for the SessionCompleted event on the Browser object.
        // This implementation writes the values returned by the VoiceXML dialog to the console.
        private void HandleSessionCompleted(object sender, SessionCompletedEventArgs e)
        {
            Console.WriteLine("VXML HandleSessionCompleted.");
            VoiceXmlResult result = e.Result;

            if (e.Result != null && e.Result.Namelist != null)
            {
                string levelName = string.Empty;
                int choice = 0;
                var selection = result.Namelist["menu"].ToString();
                Console.WriteLine("selected menu " + selection);

                currentLevel++;
                levelName = selection;

                if (currentLevel == 0)
                {
                    if (int.TryParse(selection, out choice))
                    {
                        var topLevels = parser.TopLevelMenuOptions();
                        var option = topLevels.Where(opt => opt.Id.Equals(selection,
                                             StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        levelName = option.Name;
                    }

                    PlayMenu(levelName);

                    selectedOptions.Insert(currentLevel, levelName);

                }
                else if (currentLevel == 1)
                {
                    if (int.TryParse(selection, out choice))
                    {
                        var subLevels = parser.SubOptions(selectedOptions[0]);
                        var option = subLevels.Where(opt => opt.Id.Equals(selection,
                                             StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                        levelName = option.Name;
                    }

                    selectedOptions.Insert(currentLevel, levelName);
                    waitForMenuInput.Set();
                }
            }
            else
            {
                waitForMenuInput.Set();
            }
        }


        private void PlayMenu(string menuName)
        {
            string vxmlURL = Path.Combine(vxmlDirectory, menuName.Trim().Replace(" ", "_") + ".vxml");
            Uri pageURI = new Uri(vxmlURL);
            Console.WriteLine("Browser state: " + voiceXmlBrowser.State.ToString());
            voiceXmlBrowser.RunAsync(pageURI, null);
        }

        // Handler for the Disconnecting event on the Browser object.
        private void HandleDisconnecting(object sender, DisconnectingEventArgs e)
        {
            Console.WriteLine("Disconnecting.");
        }

        // Handler for the Disconnected event on the Browser object.
        private void HandleDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected.");
        }


        public void closeResetEvents()
        {
            waitForMenuInput.Close();
            waitForAudioVideoFlowStateChangedToActiveCompleted.Close();
            waitForAudioVideoCallEstablished.Close();
        }

        public void Dispose()
        {
            closeResetEvents();

            // Calling part here
            if (voiceXmlBrowser != null)
            {
                voiceXmlBrowser.Dispose();
                voiceXmlBrowser = null;
            }
        }
    }
}
