/*=====================================================================

 File   :  DtmfMenuDialog.cs

 Summary:  Implements Dtmf menu dialog which asks user for adding a number or adding a contact   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.ConferenceService.DtmfMenuDialog
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using Microsoft.Speech.Recognition;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// DtmfMenu dialog asks user for adding number or adding contact.
    /// </summary>
    class DtmfMenuDialog : DialogBase
    {
        public enum SelectedMenuItem
        {
            None,
            AddContact,
            AddNumber,
            Failed  //Added enum if Exception is thrown by any activity.
        }

        /// <summary>
        /// Conference Service Configuration.
        /// </summary>
        public ConferenceServiceConfiguration Configuration { get; set; }

        /// <summary>
        /// AudioVideo call.
        /// </summary>
        public AudioVideoCall AudioVideoCall { get; set; }

        private Logger m_logger = new Logger();

        /// <summary>
        /// Selected menu option by user.
        /// </summary>
        private SelectedMenuItem menuItem;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        private Logger objLogger
        {
            get;
            set;
        }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of DtmfMenuDialog.
        /// </summary>
        /// <param name="audioVideoCall"></param>
        /// <param name="config"></param>
        public DtmfMenuDialog(AudioVideoCall audioVideoCall, ConferenceServiceConfiguration config, Logger logger)
        {
            this.AudioVideoCall = audioVideoCall;
            this.Configuration = config;
            this.objLogger = logger;
        }

        #endregion

        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.AudioVideoCall = inputParameters["Call"] as AudioVideoCall;
            this.Configuration = inputParameters["Configuration"] as ConferenceServiceConfiguration;
            this.objLogger = inputParameters["Logger"] as Logger;
        }

        #region Private Functions

        /// <summary>
        /// Creates dtmf grammar for menus.
        /// </summary>
        /// <returns>Grammar</returns>
        private Grammar CreateGrammar()
        {
            Grammar dtmfGrammar = new Grammar(
                   @"ConferenceService\DtmfMenuDialog\DtmfMenu.grxml",
                   "dtmfMenu");

            return dtmfGrammar;
        }

        /// <summary>
        /// Create Dtmf Grammar for help input.
        /// </summary>
        /// <returns></returns>
        private Grammar CreateHelpDtmfGrammar()
        {
            Grammar helpDtmfGrammar = new Grammar(
                 @"ConferenceService\DtmfMenuDialog\HelpMenu.grxml",
                    "help");
            return helpDtmfGrammar;
        }

        #endregion

        #region Protected Functions

        /// <summary>
        /// Create a task of speech question and answer activity for dtmf menu.
        /// </summary>
        /// <returns>Task</returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivityForDtmfMenu()
        {
            Task<ActivityResult> task = null;
            try
            {
                objLogger.Log(Logger.LogLevel.Info, "Asking dtmf menu");
                List<Grammar> dtmfGrammars = new List<Grammar> { this.CreateGrammar() };
                SpeechQuestionAnswerActivity speechQaActivity = new SpeechQuestionAnswerActivity(this.AudioVideoCall, string.Empty, null, dtmfGrammars, null, null);
                speechQaActivity.SilenceTimeOut = 3;
                speechQaActivity.MaximumSilence = 3;
                speechQaActivity.MaximumNoRecognition = 3;
                speechQaActivity.PreFlushDtmf = false;
                speechQaActivity.MainPromptAppendSssml(this.Configuration.HelpStatement.MainPrompt);
                speechQaActivity.CanBargeIn = true;
                return speechQaActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///  Create a task of speech question and answer activity for help menu
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivityForHelpMenu()
        {
            Task<ActivityResult> task = null;
            try
            {
                this.objLogger.Log(Logger.LogLevel.Info, "Wait for * to press");
                List<Grammar> dtmfGrammars = new List<Grammar> { this.CreateHelpDtmfGrammar() };
                SpeechQuestionAnswerActivity speechQaActivity = new SpeechQuestionAnswerActivity(this.AudioVideoCall, string.Empty, null, dtmfGrammars, null, null);
                speechQaActivity.SilenceTimeOut = 3;
                speechQaActivity.MaximumSilence = 3;
                speechQaActivity.MaximumNoRecognition = 3;
                speechQaActivity.PreFlushDtmf = false;
                speechQaActivity.CanBargeIn = true;
                speechQaActivity.isCommandActivity = true;
                return speechQaActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Raise the dialog completed event.
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {
            //Set the output.
            Dictionary<string, object> output = new Dictionary<string, object>();
            if (base.Exception != null)
            {
                this.menuItem = SelectedMenuItem.Failed;
            }
            output.Add("SelectedMenuItem", this.menuItem);
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));


        }
        /// <summary>
        /// Dialog completed event handler.
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }

        #endregion

        #region Public Function

        /// <summary>
        /// Activity Enumerator - Exceutes all activities of dialog one by one.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<System.Threading.Tasks.Task> GetActivities()
        {
            //Task of help command activity, waits untill user presses the '*'.
            Task<ActivityResult> helpSpeech = this.Create_SpeechQuestionAnswerActivityForHelpMenu();
            yield return helpSpeech.ContinueWith((task) =>
            {
                if (task.Exception != null)
                {
                    //Set menuitem as failed.
                    this.menuItem = SelectedMenuItem.Failed;
                }
            });

            //Ask for dtmf menu.
            Task<ActivityResult> dtmfMenuSpeech = this.Create_SpeechQuestionAnswerActivityForDtmfMenu();
            yield return dtmfMenuSpeech.ContinueWith((task) =>
            {
                if (task.Exception == null)
                {
                    RecognitionResult result = dtmfMenuSpeech.Result.Output["Result"] as RecognitionResult;
                    this.menuItem = (SelectedMenuItem)Enum.Parse(typeof(SelectedMenuItem), result.Semantics.Value.ToString(), true);

                }
                else
                {
                    //Set menuitem as failed.
                    this.menuItem = SelectedMenuItem.Failed;
                }
            });


        }

        #endregion
    }
}
