/*=====================================================================

 File   :  SendInstantMessageActivity.cs

 Summary:  Send Instant Message to the destination   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.Common.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Activity to send an instant message.
    /// </summary>
    public class SendInstantMessageActivity : ActivityBase
    {

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private InstantMessagingCall m_instantMessagingCall;
        private string m_prompt;
        private bool m_isExecuteCalled;

        /// <summary>
        /// Instant Message call object for sending messages.
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
                    throw new ArgumentNullException("Call", "SendInstantMessageActivity");
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
                    throw new ArgumentNullException("Prompt", "SendInstantMessageActivity");
            }
        }



        #region Constructors



        /// <summary>
        /// Initialize a new instance of SendInstantMessageActivity.
        /// Throws ArgumentNullException if call or prompt is null.
        /// </summary>
        /// <param name="imCall"></param>
        /// <param name="prompt"></param>
        public SendInstantMessageActivity(InstantMessagingCall imCall, string prompt)
        {
            this.InstantMessagingCall = imCall;
            this.Prompt = prompt;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            m_tcs = new TaskCompletionSource<ActivityResult>();
            Task<ActivityResult> sendInstantMessageTask =null;
            if (!m_isExecuteCalled)
            {
                sendInstantMessageTask = m_tcs.Task;
                m_isExecuteCalled = true;
                this.Run();
            }
            return sendInstantMessageTask;
        }



        /// <summary>
        /// Initialize the activity properties from parameter.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Prompt"))
                this.Prompt = parameters["Prompt"] as string;
            if (parameters.ContainsKey("Call"))
                this.InstantMessagingCall = parameters["Call"] as InstantMessagingCall;

        }

        #endregion

        /// <summary>
        /// Run the activity.
        /// </summary>
        private void Run()
        {

            var cts = new CancellationTokenSource();
            Task.Factory.FromAsync<string, SendInstantMessageResult>(
                       InstantMessagingCall.Flow.BeginSendInstantMessage,
                       InstantMessagingCall.Flow.EndSendInstantMessage,
                       this.Prompt, null
                       ).ContinueWith((t) =>
                       {
                           CancellationToken ct = cts.Token;

                           if (t.Exception != null)
                           {
                               cts.Cancel();
                               m_tcs.SetException(t.Exception);
                           }
                           else
                           {
                               m_tcs.SetResult(this.GetActivityResult());
                           }
                       }, cts.Token);

        }

        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            ActivityResult activityResult = new ActivityResult(null);
            return activityResult;
        }


    }

}
