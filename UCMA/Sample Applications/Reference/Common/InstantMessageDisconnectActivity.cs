/*=====================================================================

  File   :  InstantMessageDisconnectActivity.cs

  Summary:  Disconnects InstantMessaging Call   
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.Common.Activities
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Activity to disconnect instant messaging call.
    /// </summary>
    public class DisconnectInstantMessageCallActivity : ActivityBase
    {
        /// <summary>
        /// Instant message call to be disconnected.
        /// </summary>
        public InstantMessagingCall InstantMessagingCall
        {
            get
            {
                return instantMessagingCall;
            }
            set
            {
                if (value != null)
                    instantMessagingCall = value;
                else
                    throw new ArgumentNullException("Call", "DisconnectInstantMessageCallActivity");
            }
        }


        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> tcs ;

        private InstantMessagingCall instantMessagingCall;
        private bool m_isExecuteCalled;

        /// <summary>
        /// Initialize a new instance of InstantMessageDisconnectActivity.
        /// </summary>
        private DisconnectInstantMessageCallActivity()
        {
        }

        /// <summary>
        /// Initialize a new instance of InstantMessageDisconnectActivity.
        /// Throws ArgumentNullException if call is null.
        /// </summary>
        /// <param name="call"></param>
        public DisconnectInstantMessageCallActivity(InstantMessagingCall call)
            : this()
        {           
            this.InstantMessagingCall = call;
        }

        #region Public Functions

        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            Task<ActivityResult> disconnectInstantmessageCallTask = null;
             if (!m_isExecuteCalled)
             {
                 tcs = new TaskCompletionSource<ActivityResult>();
                disconnectInstantmessageCallTask = tcs.Task;
                m_isExecuteCalled = true;
                 this.Run();
             }
            return disconnectInstantmessageCallTask;
        }

        #endregion


        /// <summary>
        /// Runs the activity.
        /// </summary>
        private void Run()
        {
          
            Task.Factory.FromAsync(
                this.InstantMessagingCall.BeginTerminate,
                this.InstantMessagingCall.EndTerminate,
                null).ContinueWith((task) =>
                {
                    if (task.Exception != null)
                    {
                        tcs.TrySetException(task.Exception);
                    }
                    else
                    {
                        tcs.TrySetResult(this.GetActivityResult());
                    }
                });
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
