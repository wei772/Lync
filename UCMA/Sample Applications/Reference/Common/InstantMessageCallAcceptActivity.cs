/*=====================================================================

  File   :  InstantMessageCallAcceptActivity.cs

  Summary:  Accepts incomming InstantMessaging Call.   

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
    /// Activity accepts an incoming instantmessaging call.
    /// </summary>
    public class AcceptInstantMessageCallActivity : ActivityBase
    {

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private InstantMessagingCall m_instantMessagingCall;
        private bool m_isExecuteCalled;

        /// <summary>
        /// Instant message call to be accepted.
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
                    throw new ArgumentNullException("Call", "AcceptInstantMessageCallActivity");
            }
        }


        /// <summary>
        /// Initialize a new instance of InstantMessageCallAcceptActivity.
        /// </summary>
        private AcceptInstantMessageCallActivity()
        {
        }



        #region Public Function

        /// <summary>
        ///  Initialize a new instance of InstantMessageCallAcceptActivity.
        /// </summary>
        /// <param name="imCall"></param>
        public AcceptInstantMessageCallActivity(InstantMessagingCall imCall)
            : this()
        {
            this.InstantMessagingCall = imCall;
        }

        /// <summary>
        /// Initialize Parameters required for the acttivity.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Call"))
                this.InstantMessagingCall = parameters["Call"] as InstantMessagingCall;
        }

        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
             Task<ActivityResult> acceptInstantmessageCallTask = null;
             if (!m_isExecuteCalled)
             {
                 m_tcs = new TaskCompletionSource<ActivityResult>();
                 acceptInstantmessageCallTask = m_tcs.Task;
                 m_isExecuteCalled = true;
                 this.Run();
             }
            return acceptInstantmessageCallTask;
        }

        #endregion

        #region Private Functions

        private void Flow_StateChanged(object sender, MediaFlowStateChangedEventArgs e)
        {
            try
            {
                if (e.State == MediaFlowState.Terminated)
                {
                    this.UnRegisterEvents();
                    throw new InvalidOperationException("Call is terminated");
                }
            }
            catch (InvalidOperationException exception)
            {              
                
                if(m_tcs!=null)
                {                   
                    m_tcs.TrySetException(exception);
                }
            }
        }

        private void InstantMessage_InstantMessagingFlowConfigurationRequested(object sender, InstantMessagingFlowConfigurationRequestedEventArgs e)
        {
            this.InstantMessagingCall.InstantMessagingFlowConfigurationRequested -= this.InstantMessage_InstantMessagingFlowConfigurationRequested;
            InstantMessagingCall.Flow.StateChanged += new EventHandler<MediaFlowStateChangedEventArgs>(this.Flow_StateChanged);
        }

        private void InstantMessage_StateChanged(object sender, CallStateChangedEventArgs e)
        {
            try
            {
                if (e.State == CallState.Terminating || e.State == CallState.Terminated)
                {
                    this.InstantMessagingCall.StateChanged -= this.InstantMessage_StateChanged;
                    throw new InvalidOperationException("Call is terminated");
                }
            }
            catch (InvalidOperationException exception)
            {
                if(m_tcs!=null)
                {
                    m_tcs.TrySetException(exception);
                }
            }
        }

        #endregion


        /// <summary>
        /// Run the activity.
        /// </summary>
        private void Run()
        {
           

            this.InstantMessagingCall.StateChanged += new EventHandler<CallStateChangedEventArgs>(this.InstantMessage_StateChanged);

            this.InstantMessagingCall.InstantMessagingFlowConfigurationRequested += new EventHandler<InstantMessagingFlowConfigurationRequestedEventArgs>(this.InstantMessage_InstantMessagingFlowConfigurationRequested);

            Task.Factory.FromAsync<CallMessageData>(
                       InstantMessagingCall.BeginAccept,
                       InstantMessagingCall.EndAccept,
                       null).ContinueWith((task) =>
                       {
                           this.UnRegisterEvents();

                           if (task.Exception != null)
                           {                             
                               if(m_tcs!=null)
                               {                                   
                                   m_tcs.TrySetException(task.Exception);
                               }
                           }
                           else
                           {
                               if(m_tcs!=null)
                               {                                 
                                   m_tcs.TrySetResult(GetActivityResult());
                               }
                           }

                       });

        }


        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            ActivityResult activityResult = new ActivityResult( null);
            return activityResult;
        }
        /// <summary>
        /// Unregisteres events registered by Activity
        /// </summary>
        private void UnRegisterEvents()
        {
            this.InstantMessagingCall.StateChanged -= this.InstantMessage_StateChanged;

            this.InstantMessagingCall.InstantMessagingFlowConfigurationRequested -= this.InstantMessage_InstantMessagingFlowConfigurationRequested;

            this.InstantMessagingCall.Flow.StateChanged -= this.Flow_StateChanged;

        }


    }

}
