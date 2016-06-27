/*=====================================================================

 File   :  MainMenuDialog.cs

 Summary:  Implements main menu dialog   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace VoiceCompanion.MainMenu
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.VoiceCompanion;
    using Microsoft.Speech.Recognition;
    using Microsoft.Speech.Recognition.SrgsGrammar;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// Main menu dialog.
    /// </summary>
    class MainMenuDialog : DialogBase
    {
        public MainMenuConfiguration MainMenuConfiguration { get; set; }

        public AudioVideoCall avCall { get; set; }
        /// <summary>
        /// Id of selected service
        /// </summary>
        public string SelectedServiceId { get; private set; }

        private string result;
        private Logger objLogger;

        #region Constructor

        /// <summary>
        /// Initializes a new instance of MainMenu Dialog
        /// </summary>
        /// <param name="mainMenu"></param>
        /// <param name="customerSession"></param>
        public MainMenuDialog(MainMenuConfiguration mainMenu, AudioVideoCall avCall, Logger logger)
        {
            this.avCall = avCall;
            this.MainMenuConfiguration = mainMenu;
            this.objLogger = logger;
        }

        #endregion

        /// <summary>
        /// Initialize properties from inputparameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.avCall = inputParameters["Call"] as AudioVideoCall;
            this.MainMenuConfiguration = inputParameters["Configuration"] as MainMenuConfiguration;
            this.objLogger = inputParameters["Logger"] as Logger;
        }

        #region Private Function

        /// <summary>
        /// Creates a grammar for main menu options.
        /// </summary>
        /// <returns></returns>
        private Grammar SetupGrammar()
        {
            var options = this.MainMenuConfiguration.Options;

            SrgsOneOf oneOf = new SrgsOneOf();

            foreach (var option in options)
            {
                SrgsItem item = new SrgsItem(option.Index.ToString(CultureInfo.InvariantCulture));

                SrgsSemanticInterpretationTag tag =
                    new SrgsSemanticInterpretationTag("$._value = \"" + option.DtmfCode + "\";");

                SrgsItem digitItem = new SrgsItem();
                digitItem.Add(item);
                digitItem.Add(tag);

                oneOf.Add(digitItem);
            }

            SrgsRule digitRule = new SrgsRule("digit");
            digitRule.Scope = SrgsRuleScope.Public;
            digitRule.Elements.Add(oneOf);

            SrgsDocument grammarDoc = new SrgsDocument();
            grammarDoc.Mode = SrgsGrammarMode.Dtmf;
            grammarDoc.Rules.Add(digitRule);
            grammarDoc.Root = digitRule;

            return new Grammar(grammarDoc);
        }

        /// <summary>
        /// Parse result from speech question and answer activity.
        /// </summary>
        private void ParseResult(object sender, EventArgs e)
        {
            var option = this.MainMenuConfiguration.Options.FirstOrDefault(item => item.DtmfCode.Equals(result, StringComparison.OrdinalIgnoreCase));
            this.SelectedServiceId = option.ServiceId;
        }

        #endregion

        #region Protected Function

        /// <summary>
        /// Creates a task of speech question and answer activity for asking main menu. 
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_SpeechQuestionAnswerActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                objLogger.Log(Logger.LogLevel.Info, "Asking main menu");
                List<Grammar> dtmfGrammars = new List<Grammar> { this.SetupGrammar() };
                SpeechQuestionAnswerActivity speechQaActivity = new SpeechQuestionAnswerActivity(this.avCall, string.Empty, null, dtmfGrammars, null, null);
                StringBuilder prompt = new StringBuilder();
                foreach (var option in this.MainMenuConfiguration.Options)
                {
                    prompt.Append(option.Prompt);
                }
                speechQaActivity.MainPrompt = prompt.ToString();
                speechQaActivity.SilenceTimeOut = 3;
                speechQaActivity.MaximumSilence = 3;
                speechQaActivity.MaximumNoRecognition = 3;
                speechQaActivity.PreFlushDtmf = false;
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
        /// Creates a task of code activity.
        /// </summary>
        /// <returns></returns>
        protected Task<ActivityResult> Create_CodeActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(this.ParseResult);
                return codeActivity.ExecuteAsync();
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
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("ServiceId", this.SelectedServiceId);
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
        /// Execute activities of the dialog one by one.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {
            //Ask user to enter menu options.
            Task<ActivityResult> speechQuestionAnswer = this.Create_SpeechQuestionAnswerActivity();
            yield return speechQuestionAnswer.ContinueWith((task) =>
            {
                if (task.Exception == null)
                {
                    RecognitionResult recognitionResult = speechQuestionAnswer.Result.Output["Result"] as RecognitionResult;
                    this.result = recognitionResult.Text; 
                }
            });           
                
                //Parse the user's response.
                Task<ActivityResult> code = this.Create_CodeActivity();
                yield return code;
            
        }

        #endregion
    }
}
