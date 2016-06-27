/*=====================================================================

 File   :  InstantMessageQuestionAnswerActivity.cs

 Summary:  Implements Question Answer activity through InstantMessaging.
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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    /// <summary>
    /// Instant messaging question answer activity.
    /// </summary>
    public class InstantMessageQuestionAnswerActivity : ActivityBase
    {

        /// <summary>
        /// Timer for detecting silence.
        /// </summary>
        private System.Timers.Timer m_timer;

        /// <summary>
        /// Time of last received input from user.
        /// </summary>
        private DateTime m_lastImReceivedTime;


        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private InstantMessagingCall m_instantMessagingCall;
        private string m_prompt;
        private List<string> m_expectedInput;

        #region Public properties

        /// <summary>
        /// No recognition prompt, sent when user's input does not match expected input.
        /// </summary>
        public string NoRecognitionPrompt { get; set; }

        /// <summary>
        /// No recognition prompt, sent when user's input does not match expected input for maximum count.
        /// </summary>
        public string EscalatedNoRecognitionPrompt { get; set; }

        /// <summary>
        /// Prompt sent when user does not reply for specific period of time.
        /// </summary>
        public string SilencePrompt { get; set; }

        /// <summary>
        /// Prompt sent when user does not reply for specific period of time for max number of times.
        /// </summary>
        public string EscalatedSilencePrompt { get; set; }

        /// <summary>
        /// Hold prompt.
        /// </summary>
        public string PleaseHoldPrompt { get; set; }

        /// <summary>
        /// Holds users response.
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// Counter for no input from user.
        /// </summary>
        public Int16 MaxSilenceCount;
        private bool m_isExecuteCalled;

        /// <summary>
        /// Time in seconds of silence.
        /// </summary>
        public Int16 SilenceTimeOut { get; set; }

        /// <summary>
        /// Maximum No recognition count.
        /// </summary>
        public Int16 MaxNoRecoAttempts { get; set; }

        /// <summary>
        /// Count for silence.
        /// </summary>
        public Int16 NoInputCount { get; set; }

        /// <summary>
        /// Count for no recognition.
        /// </summary>
        public Int16 NoRecoCount { get; set; }

        /// <summary>
        /// Instant message call object for sending prompt.
        /// </summary>
        public InstantMessagingCall InstantMessagingCall
        {
            get
            {
                return m_instantMessagingCall;
            }
            set
            {
                if (value != null)
                    m_instantMessagingCall = value;
                else
                    throw new ArgumentNullException("Call", "InstantMessageQuestionAnswerActivity");
            }
        }

        /// <summary>
        /// Expected input used for validating users input.
        /// </summary>
        public List<string> ExpectedInput
        {
            get
            {
                return m_expectedInput;
            }
            set
            {
                if (value != null && value.Count > 0)
                    m_expectedInput = value;
                else
                    throw new ArgumentNullException("ExpectedInput", "InstantMessageQuestionAnswerActivity");
            }
        }

        /// <summary>
        /// Prompt to be sent to user.
        /// </summary>
        public string Prompt
        {
            get
            {
                return m_prompt;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_prompt = value;
                else
                    throw new ArgumentNullException("Prompt", "InstantMessageQuestionAnswerActivity");
            }
        }

        #endregion





        #region Constructors

        /// <summary>
        /// Initialize a new instance of InstantMessageQuestionAnswerActivity.
        /// </summary>
        private InstantMessageQuestionAnswerActivity()
        {
            this.MaxNoRecoAttempts = 3;
            this.MaxSilenceCount = 0;
            this.NoInputCount = 0;
            this.SilenceTimeOut = 5;
            this.PleaseHoldPrompt = string.Empty;          
            this.NoRecognitionPrompt = string.Empty;
            this.SilencePrompt = string.Empty;
            m_timer = new System.Timers.Timer(1000);
            m_timer.Elapsed += new ElapsedEventHandler(this.Timer_Elapsed);
        }



        /// <summary>
        /// Initialize a new instance of InstantMessageQuestionAnswerActivity.
        /// </summary>
        /// <param name="imCall"></param>
        /// <param name="prompt"></param>
        /// <param name="silencePrompt"></param>
        /// <param name="noRecognitionPrompt"></param>
        /// <param name="expectedInputs"></param>
        /// <param name="maxNoRecoAttempts"></param>
        /// <param name="silenceTimeOut"></param>
        public InstantMessageQuestionAnswerActivity(
            InstantMessagingCall imCall,
            string prompt,
            string silencePrompt,
            string escalatedSilencePrompt,
            string noRecognitionPrompt,
            string escalatedNoRecognitionPrompt,
            List<string> expectedInputs,
            Int16 maxNoRecoAttempts,
            Int16 silenceTimeOut,
            Int16 silenceCount)
            : this()
        {


            this.InstantMessagingCall = imCall;
            this.Prompt = prompt;
            this.ExpectedInput = expectedInputs;
            if (string.IsNullOrEmpty(noRecognitionPrompt))
                this.NoRecognitionPrompt = this.Prompt;
            else
                this.NoRecognitionPrompt = noRecognitionPrompt;

            if (string.IsNullOrEmpty(silencePrompt))
                this.SilencePrompt = this.Prompt;
            else
                this.SilencePrompt = silencePrompt;

            this.MaxSilenceCount = silenceCount;
            this.MaxNoRecoAttempts = maxNoRecoAttempts;
            this.SilenceTimeOut = silenceTimeOut;

        }

        #endregion

        #region Public Function

        /// <summary>
        /// Initialize activity properties.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Call"))
                this.InstantMessagingCall = parameters["Call"] as InstantMessagingCall;           
            if (parameters.ContainsKey("Prompt"))
                this.Prompt = parameters["Prompt"] as string;
            if (parameters.ContainsKey("ExpectedInputs"))
                this.ExpectedInput = parameters["ExpectedInputs"] as List<string>;

            if (parameters.ContainsKey("SilencePrompt"))
                this.SilencePrompt = parameters["SilencePrompt"] as string;
            else
                this.SilencePrompt = this.Prompt;

            if (parameters.ContainsKey("NoRecognitionPrompt"))
                this.NoRecognitionPrompt = parameters["NoRecognitionPrompt"] as string;
            else
                this.NoRecognitionPrompt = this.Prompt;

            if (parameters.ContainsKey("EscalatedNoRecognitionPrompt"))
                this.EscalatedNoRecognitionPrompt = parameters["EscalatedNoRecognitionPrompt"] as string;
            if (parameters.ContainsKey("EscalatedSilencePrompt"))
                this.EscalatedSilencePrompt = parameters["EscalatedSilencePrompt"] as string;
            if (parameters.ContainsKey("MaxNoRecoAttempts"))
                this.MaxNoRecoAttempts = Convert.ToInt16(parameters["MaxNoRecoAttempts"]);

            if (parameters.ContainsKey("MaxSilenceCount"))
                this.MaxSilenceCount = Convert.ToInt16(parameters["MaxSilenceCount"]);

            if (parameters.ContainsKey("SilenceTimeOut"))
                this.SilenceTimeOut = Convert.ToInt16(parameters["SilenceTimeOut"]);

        }


        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            m_tcs = new TaskCompletionSource<ActivityResult>();
            Task<ActivityResult> imQaTask = m_tcs.Task;

            if (!m_isExecuteCalled)
            {
                try
                {
                    m_isExecuteCalled = true;
                    this.Run();
                    imQaTask.Wait();
                }
                catch (AggregateException ae)
                {
                    ae.Handle((X) => { Console.WriteLine("Activity AggregateException: " + ae.InnerExceptions[0].ToString() + ae.InnerExceptions[0].Message); return true; });
                }
            }
            return imQaTask;
        }

        #endregion

        #region Protected Functions
        /// <summary>
        /// Handles an event when timer is expired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan idleInterval = DateTime.Now - this.m_lastImReceivedTime;
            Exception ex=null;
            if (idleInterval.Seconds >= this.SilenceTimeOut)
            {
                this.m_lastImReceivedTime = DateTime.Now;
                NoInputCount++;
                if (this.InstantMessagingCall.State != CallState.Terminated)
                {
                    SendInstantMessageActivity senIM = new SendInstantMessageActivity(this.InstantMessagingCall, this.NoRecognitionPrompt);
                    Task<ActivityResult> sendIMTask = senIM.ExecuteAsync();
                    sendIMTask.ContinueWith((task) =>
                    {
                        try
                        {
                            if (NoInputCount >= MaxSilenceCount)
                            {
                                this.m_timer.Stop();
                                if (!string.IsNullOrEmpty(this.EscalatedSilencePrompt))
                                {
                                    SendInstantMessageActivity sendIM = new SendInstantMessageActivity(this.InstantMessagingCall, this.EscalatedSilencePrompt);
                                    Task<ActivityResult> sendEscalatedMessage = sendIM.ExecuteAsync();
                                    sendEscalatedMessage.Wait();
                                }
                                throw new SilenceTimeOutException("No Input from User");
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
                                this.UnRegisterEvents();
                                if(m_tcs!=null)
                                {
                                    m_tcs.TrySetException(ex);
                                }
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Handles an instant message received event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Customer_MessageReceived(object sender, InstantMessageReceivedEventArgs e)
        {

            this.m_lastImReceivedTime = DateTime.Now;
            this.NoInputCount = 0;
            this.Result = e.TextBody.TrimEnd();

            if (this.NoRecoCount < this.MaxNoRecoAttempts)
            {
                if (ExpectedInput.Contains(this.Result, StringComparer.OrdinalIgnoreCase))
                {
                    this.HandleMessageReceivedComplete();
                }
                else
                {
                    this.HandleNoRecognition();
                }
            }
            else
                this.HandleMessageReceivedComplete();

        }

        /// <summary>
        /// Function called when users input is not recognized againt expected inputs.
        /// </summary>
        protected void HandleNoRecognition()
        {
            Exception ex=null;
            SendInstantMessageActivity sendInstantMessage = new SendInstantMessageActivity(this.InstantMessagingCall, this.NoRecognitionPrompt);
            Task<ActivityResult> sendNoRecoIMTask = sendInstantMessage.ExecuteAsync();
            sendNoRecoIMTask.ContinueWith((task) =>
                {
                    try
                    {
                        this.NoRecoCount++;
                        if (this.NoRecoCount == this.MaxNoRecoAttempts)
                        {
                            this.m_timer.Stop();
                            if (!string.IsNullOrEmpty(this.EscalatedNoRecognitionPrompt))
                            {
                                SendInstantMessageActivity sendIM = new SendInstantMessageActivity(this.InstantMessagingCall, this.EscalatedNoRecognitionPrompt);
                                Task<ActivityResult> sendEscalatedMessage = sendIM.ExecuteAsync();
                                sendEscalatedMessage.Wait();
                            }

                            throw new NoRecognitionException("NoRecognition");
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
                            this.UnRegisterEvents();
                            if(m_tcs!=null)
                            {
                                m_tcs.TrySetException(ex);
                            }
                        }
                    }
                });
        }

        /// <summary>
        /// Handles message received activity completed.
        /// </summary>
        protected void HandleMessageReceivedComplete()
        {
            this.UnRegisterEvents();        
            m_timer.Stop();                
            if(m_tcs!=null)
            {               
                m_tcs.TrySetResult(this.GetActivityResult());
            }
        }

      

        #endregion

        #region Private Function

        /// <summary>
        /// Handles call state changed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleInstantMessageCallStateChanged(object sender, CallStateChangedEventArgs e)
        {
            Exception exception=null;
            try
            {
                if ((e.State == CallState.Terminating || e.State == CallState.Terminated) )
                {
                    this.UnRegisterEvents();  
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
                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(exception);
                    }
                }
            }

        }



        /// <summary>
        /// Runs the activity.
        /// </summary>
        private void Run()
        {

            InstantMessagingCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(this.HandleInstantMessageCallStateChanged);


            InstantMessagingCall.Flow.MessageReceived += new EventHandler<InstantMessageReceivedEventArgs>(this.Customer_MessageReceived);

            SendInstantMessageActivity sendIMActivity = new SendInstantMessageActivity(this.InstantMessagingCall, this.Prompt);

            Task<ActivityResult> SendIMTask = sendIMActivity.ExecuteAsync();
            SendIMTask.ContinueWith((task) =>
            {
                if (task.Exception == null)
                {
                    this.m_lastImReceivedTime = DateTime.Now;
                    m_timer.Start();
                }
                else
                {                 

                    if(m_tcs!=null)
                    {
                        m_tcs.TrySetException(task.Exception);
                    }
                }
            });

        }


        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            Dictionary<string, object> output = new Dictionary<string, object>();
            if (output.Count > 0)
                output.Clear();
            output.Add("Result", this.Result);
            ActivityResult activityResult = new ActivityResult(output);
            return activityResult;
        }

        /// <summary>
        /// Unregisters events registered by this activity
        /// </summary>
        private void UnRegisterEvents()
        {
            InstantMessagingCall.Flow.MessageReceived -= this.Customer_MessageReceived;
            InstantMessagingCall.StateChanged -= this.HandleInstantMessageCallStateChanged;
        }
        #endregion
    }

}

