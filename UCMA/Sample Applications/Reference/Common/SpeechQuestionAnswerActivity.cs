/*=====================================================================

 File   :  SpeechQuestionAnswerActivity.cs

 Summary:  Implements Question Answer activity through Audio Call.
           Asks questions and waits for reply.
 
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
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Signaling;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;
    using Microsoft.Speech.Synthesis;

    /// <summary>
    /// Activity class that asks question and collect the answer in the form of audio.
    /// </summary>
    public class SpeechQuestionAnswerActivity : ActivityBase
    {
        private DtmfRecognitionEngine m_dtmfRecognizer;
        private SpeechRecognitionEngine m_speechRecognizer;
        private SpeechSynthesisConnector m_speechSynthesisConnector;
        private SpeechSynthesizer m_speechSynthesizer;
        private SpeechAudioFormatInfo m_speakAudioFormat;
        private SpeechAudioFormatInfo m_reconizeAudioFormat;
        private SpeechRecognitionConnector m_speechRecognitionConnector;
        private ToneController m_toneController;
        private SpeechRecognitionStream m_speechRecognitionStream;
        private bool m_callOnHold;
        private bool m_inputDetected;
        //Counter to check speech grammar pending to load.
        int m_pendingLoadSpeechGrammarCounter;
        //Counter to check dtmf grammar pending to load.
        int m_pendingLoadDtmfGrammarCounter;
        private bool m_isSpeakCompleted = false;
        private bool m_isSpeakGrammarLoaded = false;
        private bool m_isDtmfGrammarLoaded = false;
        //Counters for consecutive no recognition, no input attempts.
        private int m_noRecoCounter, m_silenceCounter;
        private bool m_isSilenceTimeOut;
        //The prompt to build.
        private PromptBuilder m_pbToSpeak;
        private string m_mainPrompt, m_noRecoPrompt, m_silencePromt, m_escalatedSilencePrompt, m_escalatedNoRecoPrompt;
        //The prompt to build.
        private PromptBuilder m_pbMainPrompt, m_pbNoRecoPrompt, m_pbSilencePromt, m_pbEscalatedSilencePrompt, m_pbEscalatedNoRecoPrompt;

        private TimerWheel m_timerWheel;

        private TimerItem m_timer;

        private Dictionary<string, object> m_output;

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private AudioVideoCall m_audioVideoCall;
        private bool m_isExecuteCalled;

        //Array of strings of expected dtmf inputs.
        private string[] ExpectedDtmfInputs { get; set; }
        //Array of strings of expected speech inputs.
        private string[] ExpectedSpeechInputs { get; set; }
        //List of speech grammars that will be active during recognition.
        private List<Grammar> Grammars { get; set; }
        //List of DTMF grammars that will be active during recognition.
        private List<Grammar> DtmfGrammars { get; set; }

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
                    throw new ArgumentNullException("Call", "SpeechQuestionAnswerActivity");
            }
        }
        //interval for which speech reconizer should wait for additional input before finalizing a recognition operation.
        public TimeSpan CompleteTimeOut { get; set; }
        //interval to wait between tones before terminating recognition.
        public TimeSpan InCompleteTimeOut { get; set; }
        //time length within which if user has not provided input, considered as intial silence of user.
        public int SilenceTimeOut { get; set; }
        //Flag is set if this prompt can be interrupted by the user. 
        public bool CanBargeIn { get; set; }
        //Main prompt to be played for asking question.
        public string MainPrompt { get { return m_mainPrompt; } set { m_mainPrompt = value; MainPromptAppendText(value); } }
        //Result of recognition, if recognition is invalid this will be set to null.
        public RecognitionResult RecognitionResult { get; private set; }
        //prompt to be played when user input is not recognized.
        public string NoRecognitionPrompt { get { return m_noRecoPrompt; } set { m_noRecoPrompt = value; NoRecoPromptAppendText(value); } }
        //prompt to be played when user does not give any input within specific time length.
        public string SilencePrompt { get { return m_silencePromt; } set { m_silencePromt = value; SilencePromptAppendText(value); } }
        //prompt to be played when user input is not recognized for n consecutive times.
        public string ConsecutiveSilencePrompt { get; set; }
        //prompt to be played when user does not give any input within specific time length for n consecutive times.
        public string ConsecutiveNoRecognitionPrompt { get; set; }
        //The number of times the no recognition is allowed.
        public int MaximumNoRecognition { get; set; }
        //The number of times the no input is allowed.
        public int MaximumSilence { get; set; }
        //The prompt to be played after allowed max attempt of no recognition.
        public string EscalateNoRecognitionPrompt { get { return m_escalatedNoRecoPrompt; } set { m_escalatedNoRecoPrompt = value; EscalatedNoRecoPromptAppendText(value); } }
        //The prompt to be played after allowed max attempt of silence.
        public string EscalateSilencePrompt { get { return m_escalatedSilencePrompt; } set { m_escalatedSilencePrompt = value; EscalatedSilencePromptAppendText(value); } }
        //flag is set of dtmf is to be flushed
        public bool PreFlushDtmf { get; set; }
        //This flag is set if this activity should behave as command activity.
        public bool isCommandActivity { get; set; }

        #endregion

        /// <summary>
        /// Default Constructor.
        /// </summary>
        private SpeechQuestionAnswerActivity()
        {
            this.MaximumSilence = 3;
            this.MaximumNoRecognition = 3;
            this.PreFlushDtmf = true;
            this.SilenceTimeOut = 3;
            this.CompleteTimeOut = TimeSpan.Parse("00:00:00.5");
            this.InCompleteTimeOut = TimeSpan.Parse("00:00:01");
            m_output = new Dictionary<string, object>();
            m_timerWheel = new TimerWheel();
            m_timer = new TimerItem(m_timerWheel, new TimeSpan(0, 0, 0, 0, 4000));
            m_pbToSpeak = new PromptBuilder();
        }

        /// <summary>
        /// Constructor intializes an instance of SpeechQuestionAnswerActivity.
        /// Throws ArgumentNullException if call is null or all types of grammar and expected inputs are null.
        /// </summary>
        /// <param name="avCall">Audio video call</param>
        /// <param name="mainPrompt">Main prompt to be speak</param>
        /// <param name="speechGrammar">Grammar for speech recognition</param>
        /// <param name="dtmfGrammar">Grammar for dtmf recognition</param>
        /// <param name="expectedSpeechInputs">An arry of expected speech inputs</param>
        /// <param name="expetedDtmfInputs">An arry of expected dtmf inputs</param>
        public SpeechQuestionAnswerActivity(AudioVideoCall avCall,
            string mainPrompt,
            List<Grammar> speechGrammar,
            List<Grammar> dtmfGrammar,
            string[] expectedSpeechInputs,
            string[] expetedDtmfInputs)
            : this()
        {

            this.AudioVideoCall = avCall;
            this.MainPrompt = mainPrompt;
            if (speechGrammar != null && speechGrammar.Count > 0)
                this.Grammars = speechGrammar;
            if (dtmfGrammar != null && dtmfGrammar.Count > 0)
                this.DtmfGrammars = dtmfGrammar;
            if (expectedSpeechInputs != null && expectedSpeechInputs.Length > 0)
                this.ExpectedSpeechInputs = expectedSpeechInputs;
            if (expetedDtmfInputs != null && expetedDtmfInputs.Length > 0)
                this.ExpectedDtmfInputs = expetedDtmfInputs;
            if (this.Grammars == null && this.DtmfGrammars == null && this.ExpectedSpeechInputs == null && this.ExpectedDtmfInputs == null)
                throw new ArgumentNullException("Grammar", "SpeechQuestionAnswerActivity");

        }

        /// <summary>
        /// Initialize activity properites from parameter dictionary.
        /// </summary>
        /// <param name="parameters">Key,value pair of dictionary of input parameters</param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Call"))
                this.AudioVideoCall = parameters["Call"] as AudioVideoCall;

            if (parameters.ContainsKey("MainPrompt"))
                this.MainPrompt = parameters["MainPrompt"] as string;

            if (parameters.ContainsKey("ExpectedDtmfInputs") && (parameters["ExpectedDtmfInputs"] != null && (parameters["ExpectedDtmfInputs"] as string[]).Length > 0))
                this.ExpectedDtmfInputs = (string[])parameters["ExpectedDtmfInputs"];
            if (parameters.ContainsKey("ExpectedSpeechInputs") && (parameters["ExpectedSpeechInputs"] != null && (parameters["ExpectedSpeechInputs"] as string[]).Length > 0))
                this.ExpectedSpeechInputs = (string[])parameters["ExpectedSpeechInputs"];
            if (parameters.ContainsKey("Grammars") && (parameters["Grammars"] != null && (parameters["Grammars"] as List<Grammar>).Count > 0))
                this.Grammars = (List<Grammar>)parameters["Grammars"];
            if (parameters.ContainsKey("DtmfGrammars") && (parameters["DtmfGrammars"] != null && (parameters["DtmfGrammars"] as List<Grammar>).Count > 0))
                this.DtmfGrammars = (List<Grammar>)parameters["DtmfGrammars"];

            if (this.Grammars == null && this.DtmfGrammars == null && this.ExpectedSpeechInputs == null && this.ExpectedDtmfInputs == null)
                throw new ArgumentNullException("Grammar", "SpeechQuestionAnswerActivity");


            if (parameters.ContainsKey("CompleteTimeOut") && parameters["CompleteTimeOut"] != null)
                this.CompleteTimeOut = (TimeSpan)parameters["CompleteTimeOut"];
            if (parameters.ContainsKey("InCompleteTimeOut") && parameters["InCompleteTimeOut"] != null)
                this.InCompleteTimeOut = (TimeSpan)parameters["InCompleteTimeOut"];
            if (parameters.ContainsKey("SilenceTimeOut") && parameters["SilenceTimeOut"] != null)
                this.SilenceTimeOut = (int)parameters["SilenceTimeOut"];
            if (parameters.ContainsKey("CanBargeIn") && parameters["CanBargeIn"] != null)
                this.CanBargeIn = (bool)parameters["CanBargeIn"];

            if (parameters.ContainsKey("NoRecognitionPrompt"))
                this.NoRecognitionPrompt = parameters["NoRecognitionPrompt"] as string;
            if (parameters.ContainsKey("SilencePrompt"))
                this.SilencePrompt = parameters["SilencePrompt"] as string;
            if (parameters.ContainsKey("ConsecutiveSilencePrompt"))
                this.ConsecutiveSilencePrompt = parameters["ConsecutiveSilencePrompt"] as string;
            if (parameters.ContainsKey("ConsecutiveNoRecognitionPrompt"))
                this.ConsecutiveNoRecognitionPrompt = parameters["ConsecutiveNoRecognitionPrompt"] as string;
            if (parameters.ContainsKey("MaximumNoRecognition") && parameters["MaximumNoRecognition"] != null)
                this.MaximumNoRecognition = (int)parameters["MaximumNoRecognition"];
            if (parameters.ContainsKey("MaximumSilence") && parameters["MaximumSilence"] != null)
                this.MaximumSilence = (int)parameters["MaximumSilence"];
            if (parameters.ContainsKey("EscalateNoRecognitionPrompt"))
                this.EscalateNoRecognitionPrompt = parameters["EscalateNoRecognitionPrompt"] as string;
            if (parameters.ContainsKey("EscalateSilencePrompt"))
                this.EscalateSilencePrompt = parameters["EscalateSilencePrompt"] as string;
            if (parameters.ContainsKey("PreFlushDtmf") && parameters["PreFlushDtmf"] != null)
                this.PreFlushDtmf = (bool)parameters["PreFlushDtmf"];
            if (parameters.ContainsKey("isCommandActivity") && parameters["isCommandActivity"] != null)
                this.isCommandActivity = (bool)parameters["isCommandActivity"];
        }

        #region Public methods to set prompts as in Ssml and/or text format

        public void MainPromptAppendText(string promptText)
        {
            if (m_pbMainPrompt == null)
            {
                m_pbMainPrompt = new PromptBuilder();
            }
            m_pbMainPrompt.AppendText(promptText);
            m_mainPrompt = promptText;
        }
        public void MainPromptAppendSssml(string promptSsml)
        {
            if (m_pbMainPrompt == null)
            {
                m_pbMainPrompt = new PromptBuilder();
            }
            m_pbMainPrompt.AppendSsmlMarkup(promptSsml);
        }
        public void NoRecoPromptAppendText(string promptText)
        {
            if (m_pbNoRecoPrompt == null)
            {
                m_pbNoRecoPrompt = new PromptBuilder();
            }
            m_pbNoRecoPrompt.AppendText(promptText);
        }
        public void NoRecoPromptAppendSssml(string promptSsml)
        {
            if (m_pbNoRecoPrompt == null)
            {
                m_pbNoRecoPrompt = new PromptBuilder();
            }
            m_pbNoRecoPrompt.AppendSsmlMarkup(promptSsml);
        }
        public void SilencePromptAppendText(string promptText)
        {
            if (m_pbSilencePromt == null)
            {
                m_pbSilencePromt = new PromptBuilder();
            }
            m_pbSilencePromt.AppendText(promptText);
        }
        public void SilencePromptAppendSssml(string promptSsml)
        {
            if (m_pbSilencePromt == null)
            {
                m_pbSilencePromt = new PromptBuilder();
            }
            m_pbSilencePromt.AppendSsmlMarkup(promptSsml);
        }

        public void EscalatedSilencePromptAppendText(string promptText)
        {
            if (m_pbEscalatedSilencePrompt == null)
            {
                m_pbEscalatedSilencePrompt = new PromptBuilder();
            }
            m_pbEscalatedSilencePrompt.AppendText(promptText);

        }
        public void EscalatedSilencePromptAppendSssml(string promptSsml)
        {
            if (m_pbEscalatedSilencePrompt == null)
            {
                m_pbEscalatedSilencePrompt = new PromptBuilder();
            }
            m_pbEscalatedSilencePrompt.AppendSsmlMarkup(promptSsml);
        }

        public void EscalatedNoRecoPromptAppendText(string promptText)
        {
            if (m_pbEscalatedNoRecoPrompt == null)
            {
                m_pbEscalatedNoRecoPrompt = new PromptBuilder();
            }
            m_pbEscalatedNoRecoPrompt.AppendText(promptText);
        }
        public void EscalatedNoRecoPromptAppendSssml(string promptSsml)
        {
            if (m_pbEscalatedNoRecoPrompt == null)
            {
                m_pbEscalatedNoRecoPrompt = new PromptBuilder();
            }
            m_pbEscalatedNoRecoPrompt.AppendSsmlMarkup(promptSsml);

        }
        #endregion


        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {

            m_tcs = new TaskCompletionSource<ActivityResult>();
            Task<ActivityResult> speechQATask = m_tcs.Task;

            if (!m_isExecuteCalled)
            {
                try
                {
                    m_isExecuteCalled = true;
                    this.Run();
                    speechQATask.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle((X) => { Console.WriteLine("Activity AggregateException:" + ae.InnerExceptions[0].Message); return true; });
                }
            }

            return speechQATask;
        }



        #region event handlers

        /// <summary>
        /// Handler for Silence timeout event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Timer_Expired(object sender, EventArgs e)
        {
            Exception ex = null;

            TimerItem timer = sender as TimerItem;
            timer.Stop();
            m_noRecoCounter = 0;
            //If the inteval is greater or equal to the silence time out, set initial silence timeout flag.
            m_isSilenceTimeOut = true;
            m_silenceCounter++;
            try
            {
                //If number of silence time outs does not exceed the maximum allowed silence timeout attempts.
                if (m_silenceCounter < MaximumSilence)
                {
                    //Clean up.
                    UnRegisterEvents();

                    //Set the silence prompt.
                    if (m_pbSilencePromt != null)
                        m_pbToSpeak.SetText(SilencePrompt);

                    //Restart the activity.
                    SetRecognizerandStartSpeakAsync();
                }
                else
                {
                    //Play the escalation no recognition prompt.
                    if (m_pbEscalatedSilencePrompt != null)
                        SpeakEscalation(EscalateSilencePrompt);

                    throw new SilenceTimeOutException("Escalated silence");
                }
            }
            catch (SilenceTimeOutException exception)
            {
                ex = exception;
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            finally
            {
                if (ex != null)
                {
                    //Complete the activity with exception               
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Handles event of speech detection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleSpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            m_speechRecognizer.SpeechDetected -= HandleSpeechDetected;

            //if can barg in, cancel all speak async.
            if (CanBargeIn && !m_inputDetected)
            {
                m_speechSynthesizer.SpeakAsyncCancelAll();
            }
            m_inputDetected = true;

            //Stop timer.          
            m_timer.Stop();
        }
        /// <summary>
        /// Handles event if Dtmf tone is received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleDigitDetected(object sender, ToneControllerEventArgs e)
        {
            m_dtmfRecognizer.AddTone((byte)e.Tone);

            if (CanBargeIn && !m_inputDetected)
            {
                //Cancel speak.
                m_speechSynthesizer.SpeakAsyncCancelAll();
            }
            m_inputDetected = true;
            //Stop timer.               
            m_timer.Stop();
        }

        /// <summary>
        /// Handles call state changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleAudioVideoCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            Exception exception=null;
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
                    //Complete activity with exception                 
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }

                }
            }
        }

        /// <summary>
        /// Handles terminated event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An AudioVideoFlowConfigurationChangedEventArgs that contains the event data.</param>
        protected void HandleAudioVideoConfigurationChanged(object sender, AudioVideoFlowConfigurationChangedEventArgs e)
        {
            // If the call was put on hold before the activity has started executing, 
            // then we would only get the call retrieved event.
            // In this case we safely call startspeakaync to replay the prompt.

            // However if the call was put on hold while the statement activity was 
            // executing ( in our case in between a speakasync operation) then, 
            // we should get a event for the call being put on hold, and in this case we need to cancel the speak async opeation.
            // later we will get a call retrieved event, in which case we start the speakasync operation.


            if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.Retrieved && m_callOnHold == true )
            {
                m_callOnHold = false;
                if (AudioVideoCall.State == CallState.Established)
                {                  
                    // start Speaking
                    m_pbToSpeak.ClearContent();
                    if (m_pbMainPrompt == null)
                    {
                        m_pbToSpeak.AppendBreak();
                    }
                    else
                    {
                        m_pbToSpeak = m_pbMainPrompt;
                    }
                    SetRecognizerandStartSpeakAsync();
                }
            }
            else if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.OnHold && m_callOnHold == false)
            {
                m_callOnHold = true;
                if (AudioVideoCall.State == CallState.Established)
                {
                    if (!m_isSpeakCompleted)
                        m_speechSynthesizer.SpeakAsyncCancelAll();

                    m_speechRecognizer.RecognizeAsyncCancel();

                    if (AudioVideoCall.Flow.SpeechRecognitionConnector != null)
                    {
                        AudioVideoCall.Flow.SpeechRecognitionConnector.Stop();
                        AudioVideoCall.Flow.SpeechRecognitionConnector.DetachFlow();
                    }
                    if (m_speechRecognitionStream != null)
                    {
                        m_speechRecognitionStream.Dispose();
                        m_speechRecognitionStream = null;
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
            m_speechSynthesizer.SpeakCompleted -= SpeechSynthesizer_SpeakCompleted;
            m_isSpeakCompleted = true;

            try
            {
                //Stop the synthesis connector.
                m_speechSynthesisConnector.Stop();
                m_speechSynthesisConnector.DetachFlow();
                if (e.Error != null && !(e.Error is OperationCanceledException) && !(e.Error is EndOfStreamException))
                {
                    throw e.Error;
                }
                //start timer time for silence inteval.
                if (e.Error == null && e.Cancelled == false && !m_inputDetected  && AudioVideoCall.State == CallState.Established)
                {
                    m_isSilenceTimeOut = false;
                    if (!isCommandActivity)
                        m_timer.Start();
                }

                // If we cannnot barge in then start the recognition now i.e.
                // it will start when we are done with the speak async.
                // If we can barge in then this call was done just before we start
                // the speak async process.
                if (!CanBargeIn && !m_callOnHold && AudioVideoCall.State == CallState.Established)
                {
                    m_inputDetected = false;
                    //Start speech Recognition.
                    if (Grammars != null)
                    {
                        if (Grammars.Count > 0 && m_isSpeakGrammarLoaded)
                        {
                            //Start speech recognition connector.
                            m_speechRecognitionConnector.AttachFlow(AudioVideoCall.Flow);
                            m_speechRecognitionStream = m_speechRecognitionConnector.Start();
                            //Start speech recognition engine.
                            m_speechRecognizer.SetInputToAudioStream(m_speechRecognitionStream, m_reconizeAudioFormat);
                            // Start the recognition process.
                            StartSpeechRecognizeAsync();
                        }
                    }
                    //Start Dtmf Recognition.
                    if (DtmfGrammars != null)
                    {
                        if (DtmfGrammars.Count > 0 && m_isDtmfGrammarLoaded)
                        {
                            m_dtmfRecognizer.FlushToneBuffer();
                            StartDtmfRecognizeAsync();
                        }
                    }

                }
            }
            catch (Exception exception)
            {              
                UnRegisterEvents();
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }
        }
        /// <summary>
        /// Handles speech recognizer load grammar completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleRecognizerLoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {
            //If sender is speech recognition engine, check if handler can be removed.
            if (sender is SpeechRecognitionEngine)
            {
                if (m_pendingLoadSpeechGrammarCounter > 0)
                    m_pendingLoadSpeechGrammarCounter--;
                //If all speech grammar is loaded unregister this event.
                if (m_pendingLoadSpeechGrammarCounter == 0)
                {
                    m_speechRecognizer.LoadGrammarCompleted -= HandleRecognizerLoadGrammarCompleted;
                    m_isSpeakGrammarLoaded = true;
                }

                try
                {
                    if (e.Error != null)
                    {
                        throw e.Error;
                    }
                    if (CanBargeIn && Grammars.Count > 0)
                    {
                        if (m_speechRecognitionConnector.AudioVideoFlow == null)
                        {
                            //Start speech recognition connector.               
                            m_speechRecognitionConnector.AttachFlow(AudioVideoCall.Flow);
                            m_speechRecognitionStream = m_speechRecognitionConnector.Start();
                            //Start speech recognition engine.
                            m_speechRecognizer.SetInputToAudioStream(m_speechRecognitionStream, m_reconizeAudioFormat);
                        }
                    }
                    //If we can barge in, start the speech recognition.
                    if (m_pendingLoadSpeechGrammarCounter == 0 && CanBargeIn)
                    {
                        // Start the recognition process.
                        StartSpeechRecognizeAsync();
                    }
                }
                catch (Exception exception)
                {
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }
                }
            }
        }
        /// <summary>
        /// Handles Dtmf recognizer load grammar completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleDtmfRecognizerLoadGrammarCompleted(object sender, LoadGrammarCompletedEventArgs e)
        {

            //If sender is Dtmf reconition engine, check if handler can be removed.
            if (sender is DtmfRecognitionEngine)
            {
                m_pendingLoadDtmfGrammarCounter--;
                //If all dtmf grammar is loaded unregister this event.
                if (m_pendingLoadDtmfGrammarCounter == 0)
                {
                    m_dtmfRecognizer.LoadGrammarCompleted -= HandleDtmfRecognizerLoadGrammarCompleted;
                    m_isDtmfGrammarLoaded = true;
                }
            }

            try
            {
                if (e.Error != null)
                {
                    throw e.Error;
                }
                //If we can barge in, start Dtmf recognition.
                if (m_pendingLoadDtmfGrammarCounter == 0 && CanBargeIn)
                {
                    m_dtmfRecognizer.FlushToneBuffer();
                    // Start the recognition process.
                    StartDtmfRecognizeAsync();
                }
            }
            catch (Exception exception)
            {
                UnRegisterEvents();
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }


        }
        /// <summary>
        /// Handles event for speech recognition completion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleRecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {

            //Unregister event.
            m_speechRecognizer.RecognizeCompleted -= HandleRecognizeCompleted;

            try
            {
                //Stop recognition connector.  
                if (AudioVideoCall.Flow.SpeechRecognitionConnector != null)
                {
                    AudioVideoCall.Flow.SpeechRecognitionConnector.Stop();
                    AudioVideoCall.Flow.SpeechRecognitionConnector.DetachFlow();
                }
                if (m_speechRecognitionStream != null)
                {
                    m_speechRecognitionStream.Dispose();
                    m_speechRecognitionStream = null;
                }
                //If this is not silence time out, reset the silence counter.
                if (!m_isSilenceTimeOut)
                {
                    m_silenceCounter = 0;
                }
                if (e.Error != null)
                {
                    UnRegisterEvents();
                    throw e.Error;

                }
                else if (e.Cancelled)
                {


                }
                else if (e != null && e.Result != null)
                {
                    //Recognition succeeded.
                    m_dtmfRecognizer.RecognizeAsyncCancel();
                    HandleRecognizerRecognizeSucceeded(e.Result);
                    //Reset the no recognition counter.
                    m_noRecoCounter = 0;
                }
                else  //No recognition
                {
                    m_dtmfRecognizer.RecognizeAsyncCancel();
                    //Handle Recignition rejected.
                    HandleRecognitionRejected();
                }
            }
            catch (Exception exception)
            {
                UnRegisterEvents();
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }
        }
        /// <summary>
        /// Handles event for dtmf recognition completion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandleDtmfRecognizeCompleted(object sender, DtmfRecognizeCompletedEventArgs e)
        {
            //Unregister event.
            m_dtmfRecognizer.RecognizeCompleted -= HandleDtmfRecognizeCompleted;

            try
            {
                //If this is not silence time out, reset the silence counter.
                if (!m_isSilenceTimeOut)
                {
                    m_silenceCounter = 0;
                }
                if (e.Error != null)
                {
                    UnRegisterEvents();
                    throw e.Error;
                }
                if (e.Cancelled)
                {


                }
                else if (e != null && e.Result != null)
                {
                    //Recognition succeeded.
                    m_speechRecognizer.RecognizeAsyncCancel();
                    HandleRecognizerRecognizeSucceeded(e.Result);
                    //Reset the no recognition counter.
                    m_noRecoCounter = 0;
                }
                else //No recognition
                {
                    m_speechRecognizer.RecognizeAsyncCancel();
                    if (!isCommandActivity)
                        //Handle Recignition rejected.
                        HandleRecognitionRejected();
                }
            }
            catch (Exception exception)
            {

                UnRegisterEvents();
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }
        }
        #endregion

        #region Private methods



        /// <summary>
        /// Starts the execution of speech QA activity.
        /// </summary>
        private void Run()
        {
            Exception ex = null;
            try
            {
                //Check if call is established.
                if (AudioVideoCall.State != CallState.Established)
                {
                    throw new InvalidOperationException("Call is not established");
                }

                //Checks if call is on hold.
                if (this.GetCallConfiguration(AudioVideoCall) == AvCallCommunicationEvents.OnHold)
                {
                    m_callOnHold = true;
                }
                else
                {
                    m_callOnHold = false;

                    // start Speaking.
                    m_pbToSpeak.ClearContent();
                    if (m_pbMainPrompt == null)
                    {
                        m_pbToSpeak.AppendBreak();
                    }
                    else
                    {
                        m_pbToSpeak = m_pbMainPrompt;
                    }
                    SetRecognizerandStartSpeakAsync();
                }
            }

            catch (InvalidOperationException exception)
            {
                ex = exception;
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            finally
            {
                if (ex != null)
                {
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(ex);
                    }
                }
            }
        }


        /// <summary>
        /// Checks if call is put on hold, or call is resumed from hold or call is disconnected .
        /// 
        /// </summary>
        /// <param name="call">AdioVideo call instance</param>
        /// <returns></returns>
        private AvCallCommunicationEvents GetCallConfiguration(AudioVideoCall call)
        {

            Microsoft.Rtc.Collaboration.AudioVideo.AudioChannel audioChannel = null;
            AudioVideoFlow avFlow = call.Flow;
            if (!avFlow.Audio.GetChannels().TryGetValue(ChannelLabel.AudioMono, out audioChannel))
            {
                // If we were not able to retrieve the current audio Channel is
                // becuase the call has been already disconnected.
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
        /// 1) Setup recognizers.
        /// 2) Play the Prompt.
        /// 3) Start Recognizers.
        /// </summary>
        /// <param name="prompt">prompt to be played</param>
        private void SetRecognizerandStartSpeakAsync()
        {
            //Reset all flags.
            m_callOnHold = false;
            m_inputDetected = false;
            //Counter to check speech grammar pending to load.
            m_pendingLoadSpeechGrammarCounter = 0;
            //Counter to check dtmf grammar pending to load.
            m_pendingLoadDtmfGrammarCounter = 0;
            m_isSpeakCompleted = false;
            m_isSpeakGrammarLoaded = false;
            m_isDtmfGrammarLoaded = false;
            //To register call state changed, and flow configuration changed events.            
            AudioVideoCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(HandleAudioVideoCallStateChanged);
            //Register flow configuration changed event.
            AudioVideoFlow avFlow = AudioVideoCall.Flow;
            avFlow.ConfigurationChanged += new EventHandler<AudioVideoFlowConfigurationChangedEventArgs>(HandleAudioVideoConfigurationChanged);

            //Create instances of SRC, Synthesis connector, Synthesizer and audio formats.
            m_speechRecognitionConnector = new SpeechRecognitionConnector();
            m_speakAudioFormat = new SpeechAudioFormatInfo(16000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
            m_reconizeAudioFormat = new SpeechAudioFormatInfo(8000, AudioBitsPerSample.Sixteen, Microsoft.Speech.AudioFormat.AudioChannel.Mono);
            m_speechSynthesisConnector = new SpeechSynthesisConnector();
            m_speechSynthesizer = new SpeechSynthesizer();
            m_dtmfRecognizer = new DtmfRecognitionEngine();
            m_speechRecognizer = new SpeechRecognitionEngine();
            //Set up the recognizers.
            SetupRecognizers();
            //Play the prompt.
            StartSpeakAsync();
        }


        /// <summary>
        /// Speak asyncrhonously given prompt.
        /// </summary>
        /// <param name="prompt"></param>
        private void StartSpeakAsync()
        {

            try
            {
                //Detatch flow.
                if (AudioVideoCall.Flow.SpeechSynthesisConnector != null)
                    AudioVideoCall.Flow.SpeechSynthesisConnector.DetachFlow();

                // Attach  call flow.
                m_speechSynthesisConnector.AttachFlow(AudioVideoCall.Flow);
                m_speechSynthesizer.SetOutputToAudioStream(m_speechSynthesisConnector, m_speakAudioFormat);

                //Register speak completed event.                
                m_speechSynthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(SpeechSynthesizer_SpeakCompleted);
                //Start synthesis connector.
                m_speechSynthesisConnector.Start();

                //Speak async.
                m_speechSynthesizer.SpeakAsync(m_pbToSpeak);
            }
            catch (Exception exception)
            {
                UnRegisterEvents();
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }

        }
        /// <summary>
        /// 1. Set up dtmf and speech recognizer.
        /// 2. Load grammars.
        /// 3. Set the timer for silence time out.
        /// </summary>
        private void SetupRecognizers()
        {
            m_inputDetected = false;

            if (AudioVideoCall.Flow.SpeechRecognitionConnector != null)
            {
                AudioVideoCall.Flow.SpeechRecognitionConnector.Stop();
                AudioVideoCall.Flow.SpeechRecognitionConnector.DetachFlow();
            }

            //settings for tone controller for DTMF grammar.             
            if (AudioVideoCall.Flow.ToneController == null)
            {
                m_toneController = new ToneController();
                m_toneController.AttachFlow(AudioVideoCall.Flow);
            }
            else
            {
                m_toneController = AudioVideoCall.Flow.ToneController;
            }

            m_toneController.ToneReceived += new EventHandler<ToneControllerEventArgs>(HandleDigitDetected);

            m_speechRecognizer.InitialSilenceTimeout = TimeSpan.MaxValue;

            //Set the timer for no inpu.t 
            if (SilenceTimeOut > 0)
                m_timer = new TimerItem(m_timerWheel, new TimeSpan(0, 0, 0, 0, SilenceTimeOut * 1000));

            //If this activity should behave as command activity then timer is not needed, as user can provide response at any time.
            if (!isCommandActivity)
            {
                m_timer.Expired += new EventHandler(Timer_Expired);
            }

            //Append expected input speech grammar to list of Grammars. 
            if (ExpectedSpeechInputs != null)
            {
                Grammar gExpectedInputs = GenerateGrammar(ExpectedSpeechInputs);
                Grammars.Add(gExpectedInputs);
            }
            //Append expected input dtmf grammar to list of Grammars. 
            if (ExpectedDtmfInputs != null)
            {
                Grammar gExpectedInputs = GenerateGrammar(ExpectedDtmfInputs);
                DtmfGrammars.Add(gExpectedInputs);
            }
            //Load speech grammar.
            if (Grammars != null)
            {
                if (Grammars.Count > 0)
                {
                    LoadSpeechGrammarAsync();
                }
            }
            //Load dtmf grammar.
            if (DtmfGrammars != null)
            {
                if (DtmfGrammars.Count > 0)
                {
                    m_dtmfRecognizer.InitialSilenceTimeout = TimeSpan.FromMinutes(100);
                    LoadDtmfGrammarAsync();

                }
            }
        }
        /// <summary>
        /// Loads DTMF grammar async.
        /// </summary>
        private void LoadDtmfGrammarAsync()
        {
            //set the inter tone timeout.
            m_dtmfRecognizer.InterToneTimeout = this.InCompleteTimeOut;
            //Register handler and load each grammar.
            foreach (Grammar dtmfgrammar in DtmfGrammars)
            {
                m_dtmfRecognizer.LoadGrammarCompleted += new EventHandler<LoadGrammarCompletedEventArgs>(HandleDtmfRecognizerLoadGrammarCompleted);

                m_pendingLoadDtmfGrammarCounter++;
                m_dtmfRecognizer.LoadGrammarAsync(dtmfgrammar);
            }
        }
        /// <summary>
        /// Loads speech grammar async.
        /// </summary>
        private void LoadSpeechGrammarAsync()
        {
            //Set the end silence time out.
            m_speechRecognizer.EndSilenceTimeout = this.CompleteTimeOut;
            //Register handler and load each grammar.
            foreach (Grammar grammar in Grammars)
            {
                m_speechRecognizer.LoadGrammarCompleted += new EventHandler<LoadGrammarCompletedEventArgs>(HandleRecognizerLoadGrammarCompleted);

                m_pendingLoadSpeechGrammarCounter++;
                m_speechRecognizer.LoadGrammarAsync(grammar);
            }

        }
        /// <summary>
        /// Start Dtmf recognition async.
        /// </summary>
        private void StartDtmfRecognizeAsync()
        {
            if (m_dtmfRecognizer.Grammars.Count > 0)
            {
                //Register event.               
                m_dtmfRecognizer.RecognizeCompleted += new EventHandler<DtmfRecognizeCompletedEventArgs>(HandleDtmfRecognizeCompleted);

                //Recognize dtmf async.
                try
                {
                    m_dtmfRecognizer.RecognizeAsync(PreFlushDtmf);
                }
                catch (Exception exception)
                {
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }
                }
            }
        }

        /// <summary>
        /// Method to handle if recognition is rejected.
        /// </summary>
        private void HandleRecognitionRejected()
        {
            Exception ex = null;
            //Increment the no recognition counter.
            m_noRecoCounter++;
            try
            {
                //If counter does not exceed the number of maximum allowed attempts of no recognition,
                if (m_noRecoCounter < MaximumNoRecognition)
                {
                    //Clean up.
                    UnRegisterEvents();

                    //Set the no recognition prompt.
                    if (m_pbNoRecoPrompt != null)
                        m_pbToSpeak = m_pbNoRecoPrompt;

                    //Restart the QA activity.
                    SetRecognizerandStartSpeakAsync();
                }
                else
                {
                    //Clean up
                    UnRegisterEvents();
                    //Play the escaltion no recognition prompt.
                    if (m_pbEscalatedNoRecoPrompt != null)
                        SpeakEscalation(EscalateNoRecognitionPrompt);
                    throw new NoRecognitionException("No Recongition");
                }
            }
            catch (NoRecognitionException exception)
            {
                ex = exception;
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            finally
            {
                if (ex != null)
                {
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(ex);
                    }
                }
            }
        }
        /// <summary>
        /// Start speech recognition async.
        /// </summary>
        private void StartSpeechRecognizeAsync()
        {
            if (m_speechRecognitionConnector.IsActive && m_speechRecognizer.Grammars.Count > 0)
            {
                //Register speech events.                
                m_speechRecognizer.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(HandleRecognizeCompleted);
                m_speechRecognizer.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(HandleSpeechDetected);

                //Reconize speech async.
                try
                {
                    m_speechRecognizer.RecognizeAsync(RecognizeMode.Single);
                }
                catch (Exception exception)
                {                
                    UnRegisterEvents();
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }
                }
            }
        }
        /// <summary>
        /// If recognition is succeeded.
        /// </summary>
        /// <param name="recognitionResult">recognitionResult to be sent as output of this activity</param>
        private void HandleRecognizerRecognizeSucceeded(RecognitionResult recognitionResult)
        {
            //Set the output as RecognitionResult.
            this.RecognitionResult = recognitionResult;
            m_output.Clear();
            m_output.Add("Result", this.RecognitionResult);
            //Complete the activity.

            this.UnRegisterEvents();
            if(m_tcs!=null)
            {
                m_tcs.TrySetResult(this.GetActivityResult());
            }
        }
        /// <summary>
        /// Method to generate grammar from the array of expected inputs.
        /// </summary>
        /// <param name="phrases"></param>
        /// <returns></returns>
        private Grammar GenerateGrammar(string[] phrases)
        {

            GrammarBuilder _grammarBuilder = new GrammarBuilder();

            _grammarBuilder.Append(new Choices(phrases));

            return new Grammar(_grammarBuilder);
        }

        /// <summary>
        /// Speak the escalated prompts. This method uses SpeechStatementActivity to play the prompt.
        /// </summary>
        /// <param name="prompt"></param>
        private void SpeakEscalation(string prompt)
        {
            SpeechStatementActivity speak = new SpeechStatementActivity(this.AudioVideoCall, prompt);
            Task<ActivityResult> speakEscalationTask = speak.ExecuteAsync();
            speakEscalationTask.Wait();
        }

        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            if (m_output.Count > 0)
                m_output.Clear();
            m_output.Add("Result", this.RecognitionResult);
            ActivityResult activityResult = new ActivityResult( m_output);
            return activityResult;
        }

        /// <summary>
        /// 1. Unregisters the events registered by objects of this activity
        /// 2. Cleans the speech related objects
        /// </summary>
        private void UnRegisterEvents()
        {

            //Unregister timer event
            m_timer.Expired -= Timer_Expired;

            if (m_toneController != null)
                m_toneController.ToneReceived -= HandleDigitDetected;

            AudioVideoCall.StateChanged -= HandleAudioVideoCallStateChanged;

            if (AudioVideoCall.Flow != null)
                AudioVideoCall.Flow.ConfigurationChanged -= HandleAudioVideoConfigurationChanged;


            try
            {
                //Cancel recognition
                m_dtmfRecognizer.RecognizeAsyncCancel();
                m_speechRecognizer.RecognizeAsyncCancel();
                //Unload all grammars
                m_dtmfRecognizer.UnloadAllGrammars();
                m_speechRecognizer.UnloadAllGrammars();
                //Stop the speech synthesis connector
                if (AudioVideoCall.Flow.SpeechSynthesisConnector != null)
                    m_speechSynthesisConnector.Stop();
                if (AudioVideoCall.Flow.SpeechSynthesisConnector != null)
                    AudioVideoCall.Flow.SpeechSynthesisConnector.DetachFlow();
                if (AudioVideoCall.Flow.SpeechRecognitionConnector != null)
                    AudioVideoCall.Flow.SpeechRecognitionConnector.Stop();
                if (m_speechRecognitionStream != null)
                    m_speechRecognitionStream.Dispose();
                if (AudioVideoCall.Flow.SpeechRecognitionConnector != null)
                    AudioVideoCall.Flow.SpeechRecognitionConnector.DetachFlow();
            }
            catch (Exception ex)
            {               
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(ex);
                }
            }
        }

        #endregion

    }
}
