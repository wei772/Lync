/*=====================================================================

 File   :  SkillGatheringAudioDialog.cs

 Summary:  Gathers skill from user using speech question and answer activity.   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Speech.Recognition;
    using Microsoft.Speech.Recognition.SrgsGrammar;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// SkillGathering audio dialog.
    /// </summary>
    internal class SkillGatheringAudioDialog : DialogBase
    {
        public List<AgentSkill> RequiredSkills { get { return _requiredSkills; } set { _requiredSkills = value; } }
        public AcdLogger Logger { get; set; }
        public List<string> PortalSkills { get; set; }
        public string WelcomeMessage { get; set; }
        public List<Skill> MatchMakerSkills { get; set; }
        public string PleaseHoldPrompt { get; set; }
        public string NoRecognitionPrompt { get; set; }
        public string SilencePrompt { get; set; }
        public AudioVideoCall Call { get; set; }
        private int _skillsAsked = 0;
        private Skill _currentSkill;
        private List<AgentSkill> _requiredSkills = new List<AgentSkill>();
        Dictionary<string, object> output;
        private string Result;
        private Dictionary<string, object> inputs { get; set; }
        private List<Grammar> Grammars;
        private List<Grammar> DtmfGrammars;
        private string mainPrompt;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SkillGatheringAudioDialog()
        {
            output = new Dictionary<string, object>();
        }

        /// <summary>
        /// Constructor with dictionary of input parameters.
        /// </summary>
        /// <param name="inputParameters"></param>
        public SkillGatheringAudioDialog(Dictionary<string, object> inputParameters)
        {

            this.WelcomeMessage = inputParameters["WelcomeMessage"] as string;
            this.PortalSkills = inputParameters["PortalSkills"] as List<string>;
            this.MatchMakerSkills = inputParameters["MatchMakerSkills"] as List<Skill>;
            this.PleaseHoldPrompt = inputParameters["PleaseHoldPrompt"] as string;
            this.Call = inputParameters["Call"] as AudioVideoCall;
            this._skillsAsked = 0;

            output = new Dictionary<string, object>();
        }

        /// <summary>
        /// Overrides an abstract method to intialize parameters.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.WelcomeMessage = inputParameters["WelcomeMessage"] as string;
            this.PortalSkills = inputParameters["PortalSkills"] as List<string>;
            this.MatchMakerSkills = inputParameters["MatchMakerSkills"] as List<Skill>;
            this.PleaseHoldPrompt = inputParameters["PleaseHoldPrompt"] as string;
            this.Call = inputParameters["Call"] as AudioVideoCall;
            output = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create task of speech statement activity to speak welcome message.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateSpeechActivity_SpeakWelcome()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity speakWelcome = new SpeechStatementActivity(this.Call, this.WelcomeMessage);
                return speakWelcome.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Create task of speech statement activity to speak please hold message.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateSpeechActivity_SpeakPleaseHold()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechStatementActivity speakPleaseHold = new SpeechStatementActivity(this.Call, this.PleaseHoldPrompt);
                return speakPleaseHold.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;

        }

        /// <summary>
        /// Create task of disconnect call activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateSpeechActivity_DisconnectCall()
        {
            Task<ActivityResult> task = null;
            try
            {
                DisconnectAudioCallActivity disconnectCallActivity = new DisconnectAudioCallActivity(this.Call);
                return disconnectCallActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;

        }

        /// <summary>
        /// Create task of while activity to execute condition for while loop.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateWhileActivity()
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
        /// Event handler to execute condition for while loop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Returns the result of condition</param>
        public void ExecuteWhileCondition(object sender, ConditionalEventArgs e)
        {
            e.Result = this._skillsAsked < PortalSkills.Count;
        }

        /// <summary>
        /// create a task of code activity to create the main prompt for question answer activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateCodeActivity_CreatePromptInputs()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(CreatePromptInputs_Execute);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates main prompt and intializes input dictionary for question answer activity.
        /// </summary>
        public void CreatePromptInputs_Execute(object sender, EventArgs e)
        {
            inputs = CreateInput();
            this._skillsAsked++;
        }

        /// <summary>
        /// Creates task of code activity to get the result from the answer.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateCodeActivity_ParseResult()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(ParseResult_Execute);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Parse the result from the answer of question answer activity.
        /// </summary>
        /// <returns></returns>
        public void ParseResult_Execute(object sender, EventArgs e)
        {
            AgentSkill agentSkill = null;
            if (!_currentSkill.IsValidValue(this.Result))
            {
                int index = int.Parse(Result) - 1;
                //the recognition match must be an index rather than the value itself.
                agentSkill = new AgentSkill(_currentSkill, _currentSkill.Values[index]);
                RequiredSkills.Add(agentSkill);
            }
            else if (this._currentSkill.IsValidValue(this.Result))
            {
                //the value itself was recognized.
                agentSkill = new AgentSkill(_currentSkill, this.Result);
                RequiredSkills.Add(agentSkill);
            }
        }

        /// <summary>
        /// Task of speech question answer activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateSpeechQActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                SpeechQuestionAnswerActivity speechQa = new SpeechQuestionAnswerActivity(this.Call, this.mainPrompt, this.Grammars, this.DtmfGrammars, null, null);
                speechQa.InitializeParameters(inputs);
                task= speechQa.ExecuteAsync();
                task.Wait();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            catch (AggregateException ae)
            {
                base.Exception =  ae.InnerExceptions[0];
                ae.Handle((X) => { Console.WriteLine(" SpeechQa activity aggregateException:" + ae.InnerExceptions[0].Message); return true; });
            }
            return task;
        }

        /// <summary>
        /// If we want to add any activity to the dialog flow then add it in this function.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {

            Task<ActivityResult> sendWelcomeMessage = this.CreateSpeechActivity_SpeakWelcome();
            yield return sendWelcomeMessage;


            Task<ActivityResult> whileActivity = this.CreateWhileActivity();
            yield return whileActivity;

            bool result = Convert.ToBoolean(whileActivity.Result.Output["Result"]);
            while (result)
            {
                Task<ActivityResult> codeActivity = this.CreateCodeActivity_CreatePromptInputs();
                yield return codeActivity;
                Task<ActivityResult> SpeechQA = this.CreateSpeechQActivity();

                yield return SpeechQA.ContinueWith((task) =>
                {
                    if (task.Exception != null)
                    {
                        base.Exception = task.Exception.InnerExceptions[0];
                        SilenceTimeOutException silenceException;
                        NoRecognitionException noRecognitionException = null;

                        silenceException = base.Exception as SilenceTimeOutException;
                        if (silenceException == null)
                            noRecognitionException = Exception as NoRecognitionException;
                        //If no consecutive recognition or no consecutive input then disconnect the call.
                        if (silenceException != null || noRecognitionException != null)
                        {
                            Task<ActivityResult> disconnectCall = this.CreateSpeechActivity_DisconnectCall();
                        } 
                        result = false;
                    }
                    else
                    {
                        RecognitionResult QAresult = SpeechQA.Result.Output["Result"] as RecognitionResult;
                        this.Result = QAresult.Text as string;
                    }                    
                });
           
                if (!string.IsNullOrEmpty(this.Result))
                {                   
                    Task<ActivityResult> codeActivity1 = this.CreateCodeActivity_ParseResult();
                    yield return codeActivity1;

                    whileActivity = this.CreateWhileActivity();
                    yield return whileActivity;
                    result = Convert.ToBoolean(whileActivity.Result.Output["Result"]);
                }

            }
            if (base.Exception == null)
            {
                Task<ActivityResult> pleaseHold = this.CreateSpeechActivity_SpeakPleaseHold();
                yield return pleaseHold;
            }

        }

        /// <summary>
        /// Raise a dialog completed event handler.
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {

            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("AgentSkill", RequiredSkills);
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
        /// Create inputs for SpeechQuestionAnswerActivity
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> CreateInput()
        {

            List<Grammar> skillGrammars = new List<Grammar>();
            Grammars = new List<Grammar>();
            DtmfGrammars = new List<Grammar>();
            mainPrompt = string.Empty;
            bool CanBargeIn = true;
            Dictionary<string, object> inputs = new Dictionary<string, object>();
            this._currentSkill = Skill.FindSkill(this.PortalSkills[_skillsAsked], this.MatchMakerSkills);
            skillGrammars.AddRange(CreateSkillGrammar(_currentSkill));
            Grammars.Add(skillGrammars[0]);
            DtmfGrammars.Add(skillGrammars[1]);
            mainPrompt = _currentSkill.MainPrompt;
            inputs.Add("NoRecognitionPrompt", _currentSkill.NoRecoPrompt);
            inputs.Add("SilencePrompt", _currentSkill.SilencePrompt);
            inputs.Add("SilenceTimeOut", 3);
            inputs.Add("CanBargeIn", CanBargeIn);
            inputs.Add("EscalateSilencePrompt", "I am unable to here you, please call again.");
            inputs.Add("EscalateNoRecognitionPrompt", "I am unable to understand you, please call again.");
            return inputs;

        }
        /// <summary>
        /// Create Grammar of a given skill from the config file.
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private Grammar[] CreateSkillGrammar(Skill skill)
        {
            Grammar[] grammars = new Grammar[2];

            SrgsItem[] srgsSkillValues = new SrgsItem[skill.Values.Count];
            SrgsItem[] srgsDtmfSkillValues = new SrgsItem[skill.Values.Count];

            //iterate over skill values.
            for (int i = 0; i < skill.Values.Count; i++)
            {
                //set the recognition result equal to the category name.
                SrgsSemanticInterpretationTag tag = new SrgsSemanticInterpretationTag(
                    "$._value = \"" + skill.Values[i] + "\";"
                );

                //one-of element to allow the user to enter the category name, or a number
                //representing the one-based position in the list of the category.
                SrgsOneOf categoryOneOf = new SrgsOneOf();
                //match the category name.
                categoryOneOf.Add(new SrgsItem(skill.Values[i]));
                //match the one-based index of the category in the list.
                categoryOneOf.Add(new SrgsItem((i + 1).ToString()));

                //wrap it all up with an item tag
                srgsSkillValues[i] = new SrgsItem(categoryOneOf);
                srgsSkillValues[i].Add(tag);

                srgsDtmfSkillValues[i] = new SrgsItem(i + 1);
                srgsDtmfSkillValues[i].Add(tag);
            }

            //one-of that wraps the list of items containing the category one-of elements.
            SrgsOneOf oneOf = new SrgsOneOf(srgsSkillValues);
            //root rule element.
            SrgsRule categoryRule = new SrgsRule(skill.Name.Replace(" ", ""), oneOf);
            categoryRule.Scope = SrgsRuleScope.Public;

            SrgsOneOf dtmfOneOf = new SrgsOneOf(srgsDtmfSkillValues);

            SrgsDocument grammarDoc = new SrgsDocument();
            //add and set the root rule
            grammarDoc.Rules.Add(categoryRule);
            grammarDoc.Root = categoryRule;

            //create the grammar object.
            Grammar grammar = new Grammar(grammarDoc);
            grammars[0] = grammar;


            SrgsDocument dtmfGrammarDoc = new SrgsDocument();
            dtmfGrammarDoc.Mode = SrgsGrammarMode.Dtmf;

            dtmfGrammarDoc.Rules.Add(categoryRule);
            dtmfGrammarDoc.Root = categoryRule;

            Grammar dtmfGrammar = new Grammar(dtmfGrammarDoc);
            grammars[1] = dtmfGrammar;

            return grammars;
        }
    }
}
