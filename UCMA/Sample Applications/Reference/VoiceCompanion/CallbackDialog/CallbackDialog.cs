/*=====================================================================

 File   :  CallbackDialog.cs

 Summary:  Asks user for set up a callback, if user wants to set up a callback then adds a callback request to call manager
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


namespace VoiceCompanion.CallbackDialog
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Speech.Recognition;
    using System.Globalization;
    using Microsoft.Rtc.Signaling;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices;
    using Microsoft.Rtc.Collaboration.Presence;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.Utilities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    enum CallbackAction
    {
        None = 0,
        ConnectToUser = 1,
        SetupCallback = 2
    }
    class CallbackDialog : DialogBase
    {

        #region Public properties for inputs

        public AudioVideoCall AudioVideoCall { get; set; }
        public SetupCallbackConfiguration Configuration { get; set; }
        public ContactInformation ContactInfo { get; set; }
        public Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices.GetContactService objGetContactService { get; set; }
        #endregion


        #region outputs

        private CallbackAction NextAction { get; set; }
        #endregion

        private List<Grammar> speechGrammar, dtmfGrammar;
        private bool isContactAvailable, isSuccessinCallback;

     /// <summary>
     /// Constructor
     /// </summary>
     /// <param name="avCall">audio video call</param>
     /// <param name="configuration">call back configuration</param>
     /// <param name="contactInformation">information of contact</param>
        /// <param name="getContactService">instance of GetContactService</param>
        public CallbackDialog(AudioVideoCall avCall, SetupCallbackConfiguration configuration, ContactInformation contactInformation, Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices.GetContactService getContactService)
        {
            this.AudioVideoCall = avCall;
            this.Configuration = configuration;
            this.objGetContactService = getContactService;
            this.ContactInfo = contactInformation;
            speechGrammar = new List<Grammar>();
            dtmfGrammar = new List<Grammar>();
        }

        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.AudioVideoCall = inputParameters["Call"] as AudioVideoCall;
            this.Configuration = inputParameters["Configuration"] as SetupCallbackConfiguration;
            this.objGetContactService = inputParameters["GetContactService"] as GetContactService;
            this.ContactInfo = inputParameters["ContactInformation"] as ContactInformation;

        }


        /// <summary>
        /// Get activities of dialog.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {
            bool isToSetUpCallBack=false;
            //Task checks the availibity of contact and sets the flag if contact is available.
            Task<ActivityResult> checkContactAvailibility = this.CreateCodeActivity_CheckAvailability();
            yield return checkContactAvailibility;

            if (isContactAvailable)
            {
                //Speak connecting to user statement. 
                Task<ActivityResult> stmtConnectingToUser = Create_StmtConnectingToUser();
                yield return stmtConnectingToUser;
            }
            else
            {
                //Code Activity to load grammar to ask if want to set up call back.
                Task<ActivityResult> loadGrammar = this.CreateCodeActivity_LoadGrammar();
                yield return loadGrammar;

                //Ask if want to set up a call back.
                Task<ActivityResult> speechQA = this.Create_SpeechQuestionAnswerActivity();
                yield return speechQA.ContinueWith((task) => 
                {
                    if (task.Exception != null)
                    {
                        NextAction = CallbackAction.ConnectToUser;

                        //Speak connecting to user statement. 
                        Task<ActivityResult> stmtConnectingToUser = Create_StmtConnectingToUser();
                    }
                    else
                    {
                        RecognitionResult aqResult = speechQA.Result.Output["Result"] as RecognitionResult;
                        isToSetUpCallBack = bool.Parse((string)aqResult.Semantics.Value);
                    }
                });
                if (isToSetUpCallBack)
                {
                    NextAction = CallbackAction.SetupCallback;

                    //Code activity to set up a call back.
                    isSuccessinCallback = true; //intialize as true.
                    Task<ActivityResult> setupCallback = this.CreateCodeActivity_SetupCallback();
                    yield return setupCallback;

                    //Notify customer that call back is set up.
                    Task<ActivityResult> stmtCallbackcnfrm = this.Create_stmtCallbackcnfrm();
                    yield return stmtCallbackcnfrm;
                }

            }         

        }

        /// <summary>
        /// Creates a task of speech statement activity to speak confirmation about callback.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_stmtCallbackcnfrm()
        {
            Task<ActivityResult> task = null;
            try
            {
                string confirmationPrompt = string.Format(
                   CultureInfo.InvariantCulture,
                   isSuccessinCallback ? this.Configuration.SetCallbackSucceededStatement.MainPrompt : this.Configuration.SetCallbackFailedStatement.MainPrompt,
                   this.ContactInfo.DisplayName);

                SpeechStatementActivity stmtCallbackcnfrm = new SpeechStatementActivity(this.AudioVideoCall, confirmationPrompt);
                return stmtCallbackcnfrm.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///   Creates a task of speech statement activity to speak connecting user statement
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_StmtConnectingToUser()
        {
            Task<ActivityResult> task = null;
            try
            {
                string prompt =
                   string.Format(
                   CultureInfo.InvariantCulture,
                   this.Configuration.ConnectingToUserStatement.MainPrompt,
                   this.ContactInfo.DisplayName);

                SpeechStatementActivity stmtConnectingToUser = new SpeechStatementActivity(this.AudioVideoCall, prompt);
                return stmtConnectingToUser.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///   Creates a task of code activity to load grammar for callback setup question.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_LoadGrammar()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(Code_PrepareGrammar);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///   Creates a task of code activity to setup a callback. 
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_SetupCallback()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(Code_SetupCallback);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///   Creates a task of code activity to check if contact is available.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> CreateCodeActivity_CheckAvailability()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(Code_ifContactIsAvailable);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///   Creates a task of speech question answer activity to ask question if user wants to set up a callback.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                string mainPrompt = string.Format(
                    CultureInfo.InvariantCulture,
                    this.Configuration.CallbackQa.MainPrompt,
                    this.ContactInfo.DisplayName,
                    this.ConvertAvailabilityToPrompt(this.ContactInfo.Availability));

                SpeechQuestionAnswerActivity speechGetNumber = new SpeechQuestionAnswerActivity(this.AudioVideoCall, mainPrompt, speechGrammar, null, null, null);
                speechGetNumber.NoRecognitionPrompt = this.Configuration.CallbackQa.NoRecognitionPrompt;
                speechGetNumber.SilenceTimeOut = 3;
                speechGetNumber.CanBargeIn = true;
                speechGetNumber.PreFlushDtmf = false;

                return speechGetNumber.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Converts availability status to a string phrase.
        /// </summary>
        /// <param name="availability"></param>
        /// <returns></returns>
        private string ConvertAvailabilityToPrompt(PresenceAvailability availability)
        {
            switch (availability)
            {
                case PresenceAvailability.Offline:
                    return this.Configuration.AvailabilityPrompts.OfflinePrompt;

                case PresenceAvailability.Away:
                    return this.Configuration.AvailabilityPrompts.AwayPrompt;


                case PresenceAvailability.BeRightBack:
                    return this.Configuration.AvailabilityPrompts.BeRightBackPrompt;

                case PresenceAvailability.IdleBusy:
                case PresenceAvailability.Busy:
                    return this.Configuration.AvailabilityPrompts.BusyPrompt;

                case PresenceAvailability.DoNotDisturb:
                    return this.Configuration.AvailabilityPrompts.DoNotDisturb;

                default:
                    return this.Configuration.AvailabilityPrompts.OtherPrompt;
            }
        }


        /// <summary>
        /// Creates grammar from grxml file.
        /// </summary>
        private void Code_PrepareGrammar(object sender, EventArgs e)
        {

            speechGrammar.Add(
              new Grammar(
                    @"CallbackDialog\yesno.grxml", "yesno")
            );

        }

        /// <summary>
        /// Cheks if selected contact is available.
        /// </summary>
        private void Code_ifContactIsAvailable(object sender, EventArgs e)
        {
            var availability = this.ContactInfo.Availability;

            if (availability == PresenceAvailability.IdleOnline ||
                availability == PresenceAvailability.Online)
            {

                //Contact is available.
                this.NextAction = CallbackAction.ConnectToUser;
                //Set isContactAvailable flag
                isContactAvailable = true;
            }

        }

        /// <summary>
        /// Adds a callback request for selected contact to the callbackmanager.
        /// </summary>
        private void Code_SetupCallback(object sender, EventArgs e)
        {
            var callbackManager = this.objGetContactService.CustomerSession.AppFrontEnd.CallbackManager;

            try
            {
                callbackManager.AddCallback(
                    this.objGetContactService.CustomerSession.Customer,
                    this.ContactInfo.Uri,
                    this.ContactInfo.DisplayName);
            }
            catch (RealTimeException)
            {
                isSuccessinCallback = false;
            }

        }

        /// <summary>
        /// Raise dialog completed event
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {
            //Detatch call flow.
            Helpers.DetachFlowFromAllDevices(AudioVideoCall);
            //Set the output.
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("NextAction", this.NextAction);
            output.Add("ContactURI", this.ContactInfo.Uri);
            //Raise the dialog complete event.
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));


        }
        /// <summary>
        /// Dialog completed event handler
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }
    }
}
