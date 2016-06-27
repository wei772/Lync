/*=====================================================================

 File   :  AuthenticationDialog.cs

 Summary:  Implements AuthenticationDialog to authenticate user.
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.AuthenticationDialog
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using Microsoft.Rtc.Signaling;
    using Microsoft.Speech.Recognition;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// Authenticates user based on the pin number.
    /// </summary>
    class AuthenticationDialog : DialogBase
    {
        public CustomerSession CustomerSession { get; set; }

        public AuthenticationConfiguration Configuration { get; set; }

        private int m_retries = 4;

        private bool m_isPinValid = false;

        private List<Grammar> m_speechGrammar, m_dtmfGrammar;

        private string m_pin;

        /// <summary>
        /// Gets the logger for this component.
        /// </summary>
        public Logger Logger
        {
            get
            {
                return this.CustomerSession.Logger;
            }
        }

        #region Constructor

        /// <summary>
        /// Initialize a new instance of AuthenticationDialog
        /// </summary>
        /// <param name="customerSession">Customer session</param>
        /// <param name="configuration">AuthenticationConfiguration</param>
        public AuthenticationDialog(CustomerSession customerSession, AuthenticationConfiguration configuration)
        {
            this.CustomerSession = customerSession;
            this.Configuration = configuration;
            m_speechGrammar = new List<Grammar>();
            m_dtmfGrammar = new List<Grammar>();
        }
        #endregion

        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.CustomerSession = inputParameters["CustomerSession"] as CustomerSession;
            this.Configuration = inputParameters["Configuration"] as AuthenticationConfiguration;
        }



        #region Private Functions

        /// <summary>
        /// Verify the pin entered or spoken by user
        /// </summary>
        private void ParseResults_ExecuteCode(object sender, EventArgs e)
        {
            try
            {
                m_isPinValid = false;
                Microsoft.Rtc.Collaboration.PinManagement.PinVerificationResult result =
                    this.CustomerSession.AppFrontEnd.Endpoint.PinServices.EndVerifyPin
                                        (this.CustomerSession.AppFrontEnd.Endpoint.PinServices.BeginVerifyPin(
                                                        this.CustomerSession.Customer.UserUri, m_pin, null, null, null));
                m_isPinValid = true;
            }
            catch (Microsoft.Rtc.Collaboration.PinManagement.PinFailureException)
            {
                m_isPinValid = false;
            }
            catch (InvalidOperationException ex)
            {
                this.Logger.Log(Logger.LogLevel.Error, ex);
            }
            catch (RealTimeException afx)
            {
                this.Logger.Log(Logger.LogLevel.Error, afx);
            }
        }

        /// <summary>
        /// Event handler to execute condition for while loop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Returns the result of condition</param>
        private void ExecuteWhileCondition(object sender, ConditionalEventArgs e)
        {
            e.Result = !m_isPinValid && m_retries > 0;
            m_retries--;
        }

        /// <summary>
        /// Create Grammars from grxml.
        /// </summary>
        /// 
        private void CreateGrammar(object sender, EventArgs e)
        {
            m_speechGrammar.Add(new Grammar(
                    @"AuthenticationDialog\PinVoice.grxml",
                    "fivedigits"
                ));

            m_dtmfGrammar.Add(new Grammar(
                    @"AuthenticationDialog\PinDtmf.grxml",
                    "fivedigits"
                ));
        }

        /// <summary>
        /// Build Welcome Message string.
        /// </summary>
        /// <returns></returns>
        private string GetWelcomeMessage()
        {
            var strings = this.CustomerSession.Customer.DisplayName.Split(' ');
            string firstName = string.Empty;
            if (!string.IsNullOrEmpty(strings[0]))
            {
                firstName = strings[0];
            }
            else if (!string.IsNullOrEmpty(this.CustomerSession.Customer.DisplayName))
            {
                firstName = this.CustomerSession.Customer.DisplayName;
            }

            string welcomePrompt = string.Format(
                CultureInfo.InvariantCulture,
                this.Configuration.WelcomeStatement.MainPrompt,
                firstName);
            return welcomePrompt;
        }

        /// <summary>
        /// Get message to speak at end of the dialog.
        /// </summary>
        /// <returns></returns>
        private string FinalStatemenMessage()
        {
            if (m_isPinValid)
                return this.Configuration.PinValidatedStatement.MainPrompt;
            else
                return this.Configuration.DisconnectStatement.MainPrompt;
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Raise the dialog completed event.
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {
            //Set the output.
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("UserWasAuthenticated", m_isPinValid);
            //Raise the dialog complete event.
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));

        }
        /// <summary>
        /// Dialog completed event handler.
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }


        /// <summary>
        /// Create a task of accept audio call activity of accepting the audio call.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_AudioCallAccept()
        {
            Task<ActivityResult> task = null;
            try
            {
                AcceptAudioCallActivity acceptCallActivity = new AcceptAudioCallActivity(this.CustomerSession.AudioVideoCall);
                return acceptCallActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Create a task of speech statement activity for speaking the welcome message.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechActivity(string prompt)
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity speechStatementActivity = new SpeechStatementActivity(this.CustomerSession.AudioVideoCall, prompt);
                return speechStatementActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Createa a task of while activity.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_WhileActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                WhileActivity whileActivity = new WhileActivity();
                whileActivity.ExecuteCondition += new EventHandler<ConditionalEventArgs>(ExecuteWhileCondition);
                return whileActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }



        /// <summary>
        /// Create  a task of code activity for creating grammars.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_CreatePromptInputs()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(this.CreateGrammar);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Create  a task of speech question and answer activity for asking PIN number to the user.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechQuestionAnswerActivity speechQAActivity = new SpeechQuestionAnswerActivity(this.CustomerSession.AudioVideoCall, this.Configuration.GetPinQa.MainPrompt, m_speechGrammar, m_dtmfGrammar, null, null);
                speechQAActivity.NoRecognitionPrompt = this.Configuration.GetPinQa.NoRecognitionPrompt;
                speechQAActivity.SilenceTimeOut = 3;
                speechQAActivity.MaximumSilence = 3;
                speechQAActivity.MaximumNoRecognition = 3;
                speechQAActivity.SilencePrompt = this.Configuration.GetPinQa.NoRecognitionPrompt;
                speechQAActivity.PreFlushDtmf = false;
                speechQAActivity.CanBargeIn = true;

                return speechQAActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Create code activity to parse result from speech question and answer activity.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_ParseResult()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(this.ParseResults_ExecuteCode);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        #endregion

        #region Public Function

        /// <summary>
        /// Get activities of dialog.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {
            //Accept call.
            Task<ActivityResult> acceptAudioCall = this.Create_AudioCallAccept();
            yield return acceptAudioCall;

            //Speak welcome message.
            Task<ActivityResult> sendWelcomeMessage = this.Create_SpeechActivity(this.GetWelcomeMessage());
            yield return sendWelcomeMessage;

            //Create grammar to validate pin number.
            Task<ActivityResult> createGrammer = this.CreateCodeActivity_CreatePromptInputs();
            yield return createGrammer;
            Task<ActivityResult> whileCondition = this.Create_WhileActivity();
            bool result = Convert.ToBoolean(whileCondition.Result.Output["Result"]);
            //While number of invalid attempts are not exceeded.
            while (result)
            {
                //Ask user for PIN.
                Task<ActivityResult> speechQA = this.Create_SpeechQuestionAnswerActivity();
                yield return speechQA.ContinueWith((task) =>
                    {
                        if (task.Exception != null)
                        {
                            base.Exception = task.Exception.InnerExceptions[0];
                            result = false;
                        }
                        else
                        {
                            RecognitionResult aqResult = speechQA.Result.Output["Result"] as RecognitionResult;
                            this.m_pin = (string)aqResult.Semantics.Value;
                        }
                    });

                //Parse the result to get the PIN.
                Task<ActivityResult> parseResult = this.Create_ParseResult();
                yield return parseResult.ContinueWith((task) =>
                {
                    if (task.Exception != null || !this.m_isPinValid)
                    {
                        Task<ActivityResult> inValidPin = this.Create_SpeechActivity(this.Configuration.InvalidPinStatement.MainPrompt);
                    }
                });


                whileCondition = this.Create_WhileActivity();
                result = Convert.ToBoolean(whileCondition.Result.Output["Result"]);

            }
            //Speak final message.
            Task<ActivityResult> finalSpeech = this.Create_SpeechActivity(this.FinalStatemenMessage());
            yield return finalSpeech;


        }

        #endregion
    }
}
