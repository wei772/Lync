/*=====================================================================

 File   :  CommitAgentDialog.cs

 Summary:  Implements CommitAgentDialog which asks helpdesk agent for accepting the call from user.   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
    using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

    /// <summary>
    /// Commit Agent Dialog.
    /// </summary>
    internal class CommitAgentDialog : DialogBase
    {
        private string AgentUri { get; set; }
        private AcdLogger Logger { get; set; }
        private string QaAgentOfferMainPrompt { get; set; }
        private string QaAgentOfferNoRecognitionPrompt { get; set; }
        private string QaAgentOfferSilencePrompt { get; set; }
        private bool QaAgentOfferApproval { get; set; }
        private InstantMessagingCall imCall;
        private ConversationSettings convSettings;
        private string conversationSubject = "The Microsoft Lync Server!";
        private static string conversationPriority = ConversationPriority.Urgent;
        private ApplicationEndpoint appEndPoint;
        public Guid InstanceId { get; set; }
        private string result;
        private bool agentApproval;

        #region Constructors

        /// <summary>
        /// Initialize a new instance of CommitAgentDialog.
        /// </summary>
        public CommitAgentDialog()
        {
            this.convSettings = new ConversationSettings();
            this.convSettings.Priority = conversationPriority;
            this.convSettings.Subject = conversationSubject;
            InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// Initialize a new instance of CommitAgentDialog.
        /// </summary>
        /// <param name="agentUri">Sip uri of agent</param>
        /// <param name="mainPrompt">Main prompt</param>
        /// <param name="noRecoPrompt">No recognition prompt</param>
        /// <param name="silencePrompt">Silence prompt</param>
        /// <param name="appEndPoint">Application endpoint</param>
        public CommitAgentDialog(string agentUri, string mainPrompt, string noRecoPrompt, string silencePrompt, ApplicationEndpoint appEndPoint)
            : this()
        {
            this.AgentUri = agentUri;
            this.QaAgentOfferMainPrompt = mainPrompt;
            this.QaAgentOfferNoRecognitionPrompt = noRecoPrompt;
            this.QaAgentOfferSilencePrompt = silencePrompt;
            this.appEndPoint = appEndPoint;
        }

        public CommitAgentDialog(Dictionary<string, object> inputParameters)
            : this()
        {
            this.AgentUri = inputParameters["AgentUri"] as string;
            this.QaAgentOfferMainPrompt = inputParameters["QaAgentOfferMainPrompt"] as string;
            this.QaAgentOfferNoRecognitionPrompt = inputParameters["QaAgentOfferNoRecognitionPrompt"] as string;
            this.QaAgentOfferSilencePrompt = inputParameters["QaAgentOfferSilencePrompt"] as string;
            this.appEndPoint = inputParameters["ApplicationEndPoint"] as ApplicationEndpoint;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Initialize properties from the inputParameters dictionary.
        /// </summary>
        /// <param name="inputParameters"></param>
        public override void InitializeParameters(Dictionary<string, object> inputParameters)
        {
            this.AgentUri = inputParameters["AgentUri"] as string;
            this.QaAgentOfferMainPrompt = inputParameters["QaAgentOfferMainPrompt"] as string;
            this.QaAgentOfferNoRecognitionPrompt = inputParameters["QaAgentOfferNoRecognitionPrompt"] as string;
            this.QaAgentOfferSilencePrompt = inputParameters["QaAgentOfferSilencePrompt"] as string;
            this.appEndPoint = inputParameters["ApplicationEndPoint"] as ApplicationEndpoint;
        }

        /// <summary>
        /// Creates a task of outbound instant messaging call to establish outgoing instant message call.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateOutboundImCall()
        {
            OutBoundInstantMessagingCall outboundCallActivity = new OutBoundInstantMessagingCall(this.appEndPoint, this.convSettings, this.AgentUri);
            return outboundCallActivity.ExecuteAsync();
        }

        /// <summary>
        ///  Creates a task of question answer activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateImQaActivity()
        {
            List<string> expectedInputs = new List<string> { "yes", "one", "y", "yeah", "no", "two", "n" };
            Task<ActivityResult> task = null;
            try
            {
                InstantMessageQuestionAnswerActivity imQaActivity = new InstantMessageQuestionAnswerActivity(this.imCall, this.QaAgentOfferMainPrompt, this.QaAgentOfferSilencePrompt, null, this.QaAgentOfferNoRecognitionPrompt, null, expectedInputs, 3, 10, 3);
                return imQaActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates a task of code activity to create expected inputs for instant messaging question answer.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateCodeActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                CodeActivity codeActivity = new CodeActivity();
                codeActivity.ExecuteCode += new EventHandler(this.CodeActivity);
                return codeActivity.ExecuteAsync();
            }
            catch (ArgumentNullException exception)
            {
                base.Exception = exception;
            }
            return task;
        }

        /// <summary>
        /// Creates a task of disconnect instant message call activity.
        /// </summary>
        /// <returns></returns>
        public Task<ActivityResult> CreateDisconnectImCallActivity()
        {
            Task<ActivityResult> task = null;
            try
            {
                DisconnectInstantMessageCallActivity disconnImActivity = new DisconnectInstantMessageCallActivity(this.imCall);
                return disconnImActivity.ExecuteAsync();
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
            Task<ActivityResult> outboundCall = this.CreateOutboundImCall();
            yield return outboundCall;

            this.imCall = outboundCall.Result.Output["Result"] as InstantMessagingCall;
            Task<ActivityResult> imQa = this.CreateImQaActivity();
            yield return imQa.ContinueWith((task) =>
            {
                if (task.Exception != null)
                {
                    base.Exception = task.Exception;
                    Task<ActivityResult> disc = this.CreateDisconnectImCallActivity();
                }
                else
                {
                    this.result = imQa.Result.Output["Result"] as string;
                }
            });

            
            Task<ActivityResult> code = this.CreateCodeActivity();
            yield return code;


        }

        #endregion
        /// <summary>
        /// Raise dialog completed event.
        /// </summary>
        protected override void RaiseDialogCompleteEvent()
        {

            //Set output.
            Dictionary<string, object> output = new Dictionary<string, object>();
            output.Add("Call", this.imCall);
            output.Add("InstanceId", this.InstanceId);
            output.Add("QaAgentOfferApproval", agentApproval);
            //Raise dialog completed event.
            DialogCompleteHandler(new DialogCompletedEventArgs(output, base.Exception));
        }
        /// <summary>
        /// Handles dialog completed event.
        /// </summary>
        protected override void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            base.DialogCompleteHandler(e);
        }

        /// <summary>
        /// Create Expected inputs for instant messaging question answer activity.
        /// </summary>
        private void CodeActivity(object sender, EventArgs e)
        {
            string[] acceptInputs = { "yes", "one", "y", "yeah" };
            string[] rejectInputs = { "no", "two", "n" };

            if (acceptInputs.Contains(this.result, StringComparer.OrdinalIgnoreCase))
            {
                this.agentApproval = true;
            }
            else if (rejectInputs.Contains(this.result, StringComparer.OrdinalIgnoreCase))
            {
                this.agentApproval = false;
            }

        }
    }
}
