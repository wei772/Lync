/*=====================================================================

 File   :  SkillGatheringInstantMessagingDialog.cs

 Summary:  Gathers skill from user using instant messaging question and answer activity.   
 
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
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// SkillGathering instant messaging dialog.
    /// </summary>
    internal class SkillGatheringInstantMessagingDialog : DialogBase
    {
        #region Public Properties

        /// <summary>
        /// Skills requested by user.
        /// </summary>
        public List<AgentSkill> RequiredSkills { get { return _requiredSkills; } set { _requiredSkills = value; } }

        public AcdLogger Logger { get; set; }

        /// <summary>
        /// Portal skills.
        /// </summary>
        public List<string> PortalSkills { get; set; }

        /// <summary>
        /// Welcome message.
        /// </summary>
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Match maker skills.
        /// </summary>
        public List<Skill> MatchMakerSkills { get; set; }

        /// <summary>
        /// Please hold message.
        /// </summary>
        public string PleaseHoldPrompt { get; set; }

        /// <summary>
        /// No recognition message.
        /// </summary>
        public string NoRecognitionPrompt { get; set; }

        /// <summary>
        /// Silence message.
        /// </summary>
        public string SilencePrompt { get; set; }

        /// <summary>
        /// Instant message call.
        /// </summary>
        public InstantMessagingCall ImCall { get; set; }

        #endregion

        #region Private Properties

        /// <summary>
        /// Skill asked.
        /// </summary>
        private int _skillsAsked = 0;

        /// <summary>
        /// Current skill requested by user.
        /// </summary>
        private Skill _currentSkill;

        /// <summary>
        /// Skills requested by user.
        /// </summary>
        private List<AgentSkill> _requiredSkills = new List<AgentSkill>();

        /// <summary>
        /// Users reply.
        /// </summary>
        private string imResult;

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SkillGatheringInstantMessagingDialog()
        {
            this._skillsAsked = 0;
        }

        /// <summary>
        /// Initialize a new instance of SkillGatheringInstantMessagingDialog.
        /// </summary>
        /// <param name="inputParameters"></param>
        public SkillGatheringInstantMessagingDialog(Dictionary<string, object> inputParameters)
            : this()
        {
            this.WelcomeMessage = inputParameters["WelcomeMessage"] as string;
            this.PortalSkills = inputParameters["PortalSkills"] as List<string>;
            this.MatchMakerSkills = inputParameters["MatchMakerSkills"] as List<Skill>;
            this.PleaseHoldPrompt = inputParameters["PleaseHoldPrompt"] as string;
            this.ImCall = inputParameters["Call"] as InstantMessagingCall;
        }

        #endregion

        #region Public Functions

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
            this.ImCall = inputParameters["Call"] as InstantMessagingCall;
        }

        /// <summary>
        /// Creates task of accept instant message call activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateAcceptCallTask()
        {
            Task<ActivityResult> task = null;
            try
            {
                AcceptInstantMessageCallActivity imCallAccept = new AcceptInstantMessageCallActivity(this.ImCall);
                return imCallAccept.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            catch (AggregateException aggregateException)
            {
                Exception e = aggregateException.GetBaseException();
                Console.WriteLine("AggregateException due to: " + e.InnerException);
            }
            return task;
        }

        /// <summary>
        ///  Creates task of send instant message activity to send a welcome message.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateImActivity_SendWelcome()
        {
            Task<ActivityResult> task = null;
            try
            {
                SendInstantMessageActivity sendWelcomeIM = new SendInstantMessageActivity(this.ImCall, this.WelcomeMessage);
                return sendWelcomeIM.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        ///  Creates task of send instant message activity to send a please hold message.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateImActivity_SendPleaseHoldPrompt()
        {
            Task<ActivityResult> task = null;
            try
            {
                SendInstantMessageActivity sendWelcomeIM = new SendInstantMessageActivity(this.ImCall, this.PleaseHoldPrompt);
                return sendWelcomeIM.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates task of while activity.
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
        /// Event handler to execute code condition for while loop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> returns the result of code condition.</param>
        public void ExecuteWhileCondition(object sender, ConditionalEventArgs e)
        {
            e.Result = this._skillsAsked < PortalSkills.Count;
        }

        /// <summary>
        /// Creates task of code activity to create prompts. 
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateCodeActivity_CreatePrompts()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(CreatePrompts);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Create prompts for instant messaging question and answer activity.
        /// </summary>
        public void CreatePrompts(object sender, EventArgs e)
        {
            _currentSkill = Skill.FindSkill(PortalSkills[_skillsAsked], MatchMakerSkills);
            if (null != _currentSkill)
            {
                this._skillsAsked++;
                this.WelcomeMessage = _currentSkill.MainPrompt;
                this.NoRecognitionPrompt = _currentSkill.NoRecoPrompt;
                this.SilencePrompt = _currentSkill.SilencePrompt;
            }
        }

        /// <summary>
        /// Creates a task of code activity. 
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateCodeActivity_ParseResult()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(ParseResult);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Parse result from instant messaging question and answer activity.
        /// </summary>
        public void ParseResult(object sender, EventArgs e)
        {
            AgentSkill agentSkill = null;
            if (!_currentSkill.IsValidValue(this.imResult))
            {
                int index = int.Parse(imResult) - 1;
                //the recognition match must be an index rather than the value itself.
                agentSkill = new AgentSkill(_currentSkill, _currentSkill.Values[index]);
                RequiredSkills.Add(agentSkill);
            }
            else if (this._currentSkill.IsValidValue(this.imResult))
            {
                //the value itself was recognized.
                agentSkill = new AgentSkill(_currentSkill, this.imResult);
                RequiredSkills.Add(agentSkill);
            }
        }

        /// <summary>
        /// Creates a task of instant messaging q uestion/Answer activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateQaImActivity()
        {
            Task<ActivityResult> task = null;
            try
            {

                InstantMessageQuestionAnswerActivity imQa = new InstantMessageQuestionAnswerActivity(this.ImCall, this.WelcomeMessage, this.SilencePrompt, null, this.NoRecognitionPrompt, null, this._currentSkill.Values, 3, 15, 3);
                return imQa.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Get activities of dialog.
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Task> GetActivities()
        {

            Task<ActivityResult> acceptCall = this.CreateAcceptCallTask();
            yield return acceptCall;

            Task<ActivityResult> welcome = this.CreateImActivity_SendWelcome();
            yield return welcome;

            Task<ActivityResult> whileActivity = this.CreateWhileActivity();
            yield return whileActivity;

            bool result = Convert.ToBoolean(whileActivity.Result.Output["Result"]);
            while (result)
            {
                Task<ActivityResult> codeActivity = this.CreateCodeActivity_CreatePrompts();
                yield return codeActivity;
                Task<ActivityResult> imqa = this.CreateQaImActivity();
                yield return imqa.ContinueWith((task) =>
               {
                   if (task.Exception != null)
                   {                      
                       result = false;
                   }
                   else
                   {
                       this.imResult = imqa.Result.Output["Result"] as string;
                   }
               });

                
                Task<ActivityResult> codeActivity1 = this.CreateCodeActivity_ParseResult();
                yield return codeActivity1;

                whileActivity = this.CreateWhileActivity();
                yield return whileActivity;
                result = Convert.ToBoolean(whileActivity.Result.Output["Result"]);

            }

            Task<ActivityResult> pleaseHold = this.CreateImActivity_SendPleaseHoldPrompt();
            yield return pleaseHold;

        }

        #endregion

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

    }
}
