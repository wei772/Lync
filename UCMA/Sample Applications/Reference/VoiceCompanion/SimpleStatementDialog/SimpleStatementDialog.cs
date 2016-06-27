/*=====================================================================

 File   :  SimpleStatementDialog.cs

 Summary:  Implements SimpleStatementDialog Dialog which just speaks a given statement.   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.SimpleStatementDialog
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    class SimpleStatementDialog : DialogBase
    {
        /// <summary>
        /// Main prompt to be speak.
        /// </summary>
        public string MainPrompt { get; set; }

        private AudioVideoCall AudioVideoCall { get; set; }
        private Logger m_logger = new Logger();

        public Logger Logger
        {
            get
            {
                return m_logger;
            }
        }

        public SimpleStatementDialog(string prompt, AudioVideoCall call)
        {
            this.MainPrompt = prompt;
            this.AudioVideoCall = call;
        }


        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.MainPrompt = inputParameters["MainPrompt"] as string;
            this.AudioVideoCall = inputParameters["Call"] as AudioVideoCall;
        }

        /// <summary>
        /// If we want to add any activity to the dialog flow then add it in this function.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<System.Threading.Tasks.Task> GetActivities()
        {
            Task<ActivityResult> speechStat = this.Create_SpeechStatement();
            yield return speechStat;
        }

        /// <summary>
        /// Create a task of speech statement activity.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechStatement()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity speechStatementActivity = new SpeechStatementActivity(this.AudioVideoCall, this.MainPrompt);
                return speechStatementActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///Raise the dialog complete event.
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {          
            this.DialogCompleteHandler(new DialogCompletedEventArgs(null, base.Exception));
        }
        /// <summary>
        /// Dialog completed event handler.
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }
    }
}
