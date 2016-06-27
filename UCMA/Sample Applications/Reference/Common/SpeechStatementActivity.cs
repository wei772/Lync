/*=====================================================================

  File   :  SpeechStatementActivity.cs

  Summary:  Speaks given prompt to destination through audio video call.   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.Common.Activities
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Synthesis;
    using System.Threading.Tasks;


    /// <summary>
    /// Speaks given prompt to destination.
    /// </summary>
    public class SpeechStatementActivity : ActivityBase
    {

        #region Private variables
        private SpeechAudioFormatInfo m_audioformat;
        private SpeechSynthesizer m_speechSynthesizer;
        private SpeechSynthesisConnector m_speechSynthesisConnector;
        private bool m_callOnHold;
        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;
        
        private string m_mainPrompt;
        //The prompt to build
        private PromptBuilder m_pbMainPrompt;
        private AudioVideoCall m_audioVideoCall;
private  bool m_isExecuteCalled;
        #endregion
        #region Public Properties
        public AudioVideoCall AudioVideoCall
        {
            get
            {
                return m_audioVideoCall;
            }
            set
            {
                if (value != null)
                    m_audioVideoCall = value;
                else
                    throw new ArgumentNullException("Call", "SpeechStatementActivity");
            }
        }
        public string Prompt { get { return m_mainPrompt; } set { m_mainPrompt = value; MainPromptAppendText(value); } }
        #endregion



        /// <summary>
        /// Default Constructor
        /// </summary>
        private SpeechStatementActivity()
        {
            m_speechSynthesisConnector = new SpeechSynthesisConnector();
            m_speechSynthesizer = new SpeechSynthesizer();
            m_audioformat = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
        }

        /// <summary>
        /// Constructor- Initializes an instance of SpeechStatementActivity.
        /// Throws argument null exception if call is null. If prompt is null a it speaks a pause
        /// </summary>
        /// <param name="call">An instance of audiovideocall</param>
        /// <param name="prompt">prompt to be speak</param>
        public SpeechStatementActivity(AudioVideoCall call, string prompt)
            : this()
        {
            this.AudioVideoCall = call;
            this.Prompt = prompt;

        }

        /// <summary>
        /// Initialize activity properties from parameters dictionary.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Prompt"))
                this.Prompt = parameters["Prompt"] as string;
            if (parameters.ContainsKey("Call"))
                this.AudioVideoCall = parameters["Call"] as AudioVideoCall;

        }



        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            m_tcs = new TaskCompletionSource<ActivityResult>();
             Task<ActivityResult> speechStatementTask =null;

             if (!m_isExecuteCalled)
             {
                  try
                {
                 speechStatementTask = m_tcs.Task;
                 m_isExecuteCalled = true;
                 this.Run();
                 speechStatementTask.Wait();
                }
                  catch (AggregateException ae)
                  {
                      ae.Handle((X) => { Console.WriteLine("Activity AggregateException: " + ae.InnerExceptions[0].ToString() + ae.InnerExceptions[0].Message); return true; });
                  }
             }
            return speechStatementTask;
        }



        /// <summary>
        /// Append message in text format to main prompt
        /// </summary>
        /// <param name="promptText"></param>
        public void MainPromptAppendText(string promptText)
        {
            if (m_pbMainPrompt == null)
            {
                m_pbMainPrompt = new PromptBuilder();
            }
            m_pbMainPrompt.AppendText(promptText);
            m_mainPrompt = promptText;
        }

        /// <summary>
        /// Append message in SSML format to main prompt.
        /// </summary>
        /// <param name="promptSsml"></param>
        public void MainPromptAppendSssml(string promptSsml)
        {
            if (m_pbMainPrompt == null)
            {
                m_pbMainPrompt = new PromptBuilder();
            }
            m_pbMainPrompt.AppendSsmlMarkup(promptSsml);
        }

        #region Event Handlers

        /// <summary>
        /// Handles flow configuration changed event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An AudioVideoFlowConfigurationChangedEventArgs that contains the event data.</param>
        private void HandleAudioVideoConfigurationChanged(object sender, AudioVideoFlowConfigurationChangedEventArgs e)
        {
            // If the call was put on hold before the activity has started executing, 
            // then we would only get the call retrieved event.
            // In this case we safely call startspeakaync to replay the prompt.

            // However if the call was put on hold while the statement activity was 
            // executing ( in our case in between a speakasync operation) then, 
            // we should get a event for the call being put on hold, and in this case we need to cancel the speak async opeation.
            // later we will get a call retrieved event, in which case we start the speakasync operation.


            if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.Retrieved && m_callOnHold == true)
            {
                m_callOnHold = false;
                if (AudioVideoCall.State == CallState.Established)
                {
                    StartSpeakAsync();
                }
            }
            else if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.OnHold && m_callOnHold == false)
            {
                m_callOnHold = true;
                if (AudioVideoCall.State == CallState.Established)
                {
                    m_speechSynthesizer.SpeakAsyncCancelAll();
                }
            }
        }

        /// <summary>
        /// Handles call state changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleAudioVideoCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            Exception exception = null;
            try
            {
                if (e.State == CallState.Terminating || e.State == CallState.Terminated) 
                {
                    throw new InvalidOperationException("Call is terminated");
                }
            }
            catch (InvalidOperationException ex)
            {
                exception = ex;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (exception != null)
                {
                    this.UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }

                }
            }

        }


        /// <summary>
        /// Handles speak completed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void SpeechSynthesizer_SpeakCompleted(object sender, SpeakCompletedEventArgs e)
        {
            Exception exception = null;
            try
            {
                m_speechSynthesizer.SpeakCompleted -= SpeechSynthesizer_SpeakCompleted;

                if (e.Error != null && !(e.Error is OperationCanceledException) && !(e.Error is EndOfStreamException))
                {
                    throw e.Error;
                }
                if (m_callOnHold == true && e.Cancelled == true)
                {
                    // Do not complete the activity if we are on hold and speakcancel was called.
                    return;
                }
              
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                this.UnRegisterEvents();
                //Complete the activity.
                if (exception != null)
                {
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }
                }
                else
                {
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetResult(this.GetActivityResult());
                    }
                }
            }
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Runs the activity.
        /// </summary>
        private void Run()
        {
            //Check if call is established
            try
            {

                if (AudioVideoCall.State != CallState.Established)
                    throw new InvalidOperationException("Call is not established");

                //Register flow configuration changed event. 
                AudioVideoFlow avFlow = AudioVideoCall.Flow;
                avFlow.ConfigurationChanged += new EventHandler<AudioVideoFlowConfigurationChangedEventArgs>(HandleAudioVideoConfigurationChanged);

                AudioVideoCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(HandleAudioVideoCallStateChanged);

                //Checks if call is on hold.
                if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.OnHold)
                {
                    m_callOnHold = true;

                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetResult(this.GetActivityResult());
                    }
                }
                else
                {
                    m_callOnHold = false;
                    // start Speaking.
                    StartSpeakAsync();
                }
            }
            catch (InvalidOperationException exception)
            {
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }
        }

        /// <summary>
        ///Starts speech connector and starts speaking the prompt message asynchronously.
        /// </summary>
        private void StartSpeakAsync()
        {
            //start Speech synthesizer
            if (AudioVideoCall.Flow.SpeechSynthesisConnector != null)
                AudioVideoCall.Flow.SpeechSynthesisConnector.DetachFlow();
            m_speechSynthesisConnector.AttachFlow(AudioVideoCall.Flow);
            m_speechSynthesizer.SetOutputToAudioStream(m_speechSynthesisConnector, m_audioformat);
            m_speechSynthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(SpeechSynthesizer_SpeakCompleted);
            m_speechSynthesisConnector.Start();
            //If prompt is not set then speak Ssml pause.
            if (m_pbMainPrompt == null)
            {
                m_pbMainPrompt = new PromptBuilder();
                m_pbMainPrompt.AppendBreak();
            }
            m_speechSynthesizer.SpeakAsync(m_pbMainPrompt);
        }

        /// <summary>
        /// Checks if call is put on hold, or call is resumed from hold or call is disconnected. 
        /// 
        /// </summary>
        /// <param name="call">AdioVideo call instant.</param>
        /// <returns>string</returns>
        private AvCallCommunicationEvents GetCallConfiguration(AudioVideoCall call)
        {

            Microsoft.Rtc.Collaboration.AudioVideo.AudioChannel audioChannel = null;
            AudioVideoFlow avFlow = call.Flow;
            if (!avFlow.Audio.GetChannels().TryGetValue(ChannelLabel.AudioMono, out audioChannel))
            {
                // if we were not able to retrieve the current audio Channel is
                // becuase the call has been already disconnected
                return AvCallCommunicationEvents.Disconnected;
            }

            MediaChannelDirection direction = audioChannel.Direction;

            if (direction == MediaChannelDirection.SendReceive ||
               direction == MediaChannelDirection.ReceiveOnly)
            {
                return AvCallCommunicationEvents.Retrieved;
            }
            else if (direction == MediaChannelDirection.Inactive ||
               direction == MediaChannelDirection.SendOnly)
            {
                return AvCallCommunicationEvents.OnHold;
            }

            return AvCallCommunicationEvents.None;

        }

        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            ActivityResult activityResult = new ActivityResult(null);
            return activityResult;
        }

        /// <summary>
        /// method for clean up and unregistering events.
        /// </summary>
        private void UnRegisterEvents()
        {
            m_speechSynthesisConnector.Stop();
            if (AudioVideoCall.Flow.SpeechSynthesisConnector != null)
                AudioVideoCall.Flow.SpeechSynthesisConnector.DetachFlow();

            if (AudioVideoCall != null)
            {
                AudioVideoCall.StateChanged -= HandleAudioVideoCallStateChanged;
                if (AudioVideoCall.Flow != null)
                {
                    AudioVideoCall.Flow.ConfigurationChanged -= HandleAudioVideoConfigurationChanged;
                }
            }
        }


        #endregion


    }

}
