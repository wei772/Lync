/*=====================================================================

  File   :  AudioCallAcceptActivity.cs

  Summary:  Accepts AudioVideo Call   
 
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
    using Microsoft.Rtc.Collaboration;
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using System.Collections.Generic;

    public class AcceptAudioCallActivity : ActivityBase
    {
        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private AudioVideoCall m_audioVideoCall;

        private bool m_isExecuteCalled;

        /// <summary>
        /// An instance of audiovideo call
        /// </summary>
        public AudioVideoCall AudioVideoCall
        {
            get
            {
                return m_audioVideoCall;
            }
            set
            {
                if (value != null)
                    m_audioVideoCall = value;
                else
                    throw new ArgumentNullException("Call", "AcceptAudioCallActivity");
            }
        }


        /// <summary>
        /// Constructor for this class
        /// throws ArgumentNullException if audioCall is null
        /// </summary>
        /// <param name="audioCall">an audiovideo call</param>
        public AcceptAudioCallActivity(AudioVideoCall avCall)
        {
            this.AudioVideoCall = avCall;
        }

        /// <summary>
        /// Initialize Parameters for the activity.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("Call"))
                this.AudioVideoCall = parameters["Call"] as AudioVideoCall;
        }

        /// <summary>
        /// Runs an activity to begin its execution
        /// </summary>
        private void Run()
        {

            Task.Factory.FromAsync<CallMessageData>(
                AudioVideoCall.BeginAccept,
                AudioVideoCall.EndAccept,
                null).ContinueWith((task) =>
                    {
                        if (task.Exception != null)
                        {
                            m_tcs.TrySetException(task.Exception);
                        }
                        else
                        {  
                            m_tcs.TrySetResult(this.GetActivityResult());
                        }
                    }
                    );

        }


        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            Task<ActivityResult> acceptAudioCallTask = null;
            if (!m_isExecuteCalled)
            {
                m_tcs = new TaskCompletionSource<ActivityResult>();
                acceptAudioCallTask = m_tcs.Task;
                m_isExecuteCalled = true;
                this.Run();
                
            }
            return acceptAudioCallTask;
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
