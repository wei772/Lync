
/*=====================================================================

 File   :  DialupDialog.cs

 Summary:  Gets the number to dial from customer
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.DialupDialog
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Speech.Recognition;
    using System.Globalization;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    class DialupDialog : DialogBase
    {
        #region Public properties for inputs to Dialog
        public AudioVideoCall AudioVideoCall { get; set; }

        public DialupConfiguration Configuration { get; set; }
        #endregion


        /// <summary>
        ///  output of this dialog is the number user wants to dial
        /// </summary>
        private string Number { get; set; }

        private List<Grammar> speechGrammar, dtmfGrammar;



        /// <summary>
        ///Constructor
        /// </summary>
        /// <param name="avCall"></param>
        /// <param name="configuration"></param>
        public DialupDialog(AudioVideoCall avCall, DialupConfiguration configuration)
        {
            this.AudioVideoCall = avCall;
            this.Configuration = configuration;
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
            this.Configuration = inputParameters["Configuration"] as DialupConfiguration;
        }

        /// <summary>
        /// Get activities of dialog. Executes activities of the dialog one by one.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {

            //Create task to load grammar for question-answer to get the number.
            Task<ActivityResult> createGrammer = this.CreateCodeActivity_LoadGrammar();
            yield return createGrammer;
            //Get the number from customer.
            Task<ActivityResult> speechQA = this.Create_SpeechQuestionAnswerActivity();
            yield return speechQA.ContinueWith((task) =>
            {
                if (task.Exception == null)
                {
                    RecognitionResult aqResult = speechQA.Result.Output["Result"] as RecognitionResult;
                    this.Number = (string)aqResult.Semantics.Value;
                }
            });

            //Confirm the number.
            Task<ActivityResult> confirmNumberSpeech = this.Create_StmtConfirmNumber();
            yield return confirmNumberSpeech;

        }

        /// <summary>
        /// Creates a task of speech statement activity to confirm the number.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_StmtConfirmNumber()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity confirmNumberSpeech = new SpeechStatementActivity(this.AudioVideoCall, string.Empty);
                confirmNumberSpeech.MainPromptAppendText(this.Configuration.NumberConfirmationStatement.MainPrompt);
                string ssml = string.Format(CultureInfo.InvariantCulture, "<prosody rate=\"slow\">{0}</prosody>", this.Number);
                confirmNumberSpeech.MainPromptAppendSssml(ssml);
                return confirmNumberSpeech.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }


        /// <summary>
        ///  Creates a task of code activity to load grammar.
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
        ///  Creates a task of speech question answer activity to ask user for number to dial.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechQuestionAnswerActivity speechGetNumber = new SpeechQuestionAnswerActivity(this.AudioVideoCall, this.Configuration.GetNumberQA.MainPrompt, speechGrammar, dtmfGrammar, null, null);

                speechGetNumber.NoRecognitionPrompt = this.Configuration.GetNumberQA.NoRecognitionPrompt;
                speechGetNumber.SilenceTimeOut = 3;
                speechGetNumber.CanBargeIn = true;
                speechGetNumber.PreFlushDtmf = true;
                return speechGetNumber.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }




        /// <summary>
        /// Creates Grammars from grxml.
        /// </summary>
        private void Code_PrepareGrammar(object sender, EventArgs e)
        {

            speechGrammar.Add(
                new Grammar(
                    @"DialupDialog\PhoneNumberVoice.grxml",
                    "phonenumber"
                )
            );


            dtmfGrammar.Add(
                new Grammar(
                    @"DialupDialog\PhoneNumberDtmf.grxml",
                    "phonenumber"
                )
            );
        }


        /// <summary>
        /// Raises the dialog completion event 
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {
            //set the output.
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("Number", Number);
            //raise the dialog complete event.
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));

        }
        /// <summary>
        /// Dialog complete handler.
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }

    }
}
