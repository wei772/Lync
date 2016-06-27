/*=====================================================================

  File   :  AudioDisconnectCallActivity.cs

  Summary:  Disconnects AudioVideo Call   
 
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
    using Microsoft.Rtc.Collaboration.AudioVideo;
    using Microsoft.Rtc.Collaboration.Samples.Utilities;

    /// <summary>
    /// An activity to disconnect an audio video call.
    /// </summary>
    public class DisconnectAudioCallActivity : ActivityBase
    {

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        private AudioVideoCall m_audioVideoCall;
        private  bool m_isExecuteCalled;

        /// <summary>
        /// AudioVideo call to be disconnected.
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
                    throw new ArgumentNullException("Call", "DisconnectAudioCallActivity");
            }
        }




        #region Constructors

        /// <summary>
        /// Initializes a new instance of AudioDisconnectCallActivity.
        /// </summary>
        /// <param name="avCall"></param>
        public DisconnectAudioCallActivity(AudioVideoCall avCall)
        {   
            AudioVideoCall = avCall;
        }

        #endregion

        #region Public Function
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
        /// Run the activity.
        /// </summary>
        private void Run()
        {
          
            Task.Factory.FromAsync(
                       AudioVideoCall.BeginTerminate,
                       AudioVideoCall.EndTerminate,
                       null).ContinueWith((t) =>
                           {        
                       
                               
                                   if (t.Exception != null)
                                   {

                                       m_tcs.TrySetException(t.Exception);
                                   }
                                   else
                                   {                                       
                                       m_tcs.TrySetResult(this.GetActivityResult());
                                   }                              
                               
                           });
            
        }

        
        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            Task<ActivityResult> disconnectAudioCallTask = null;
            if (!m_isExecuteCalled)
            {
                m_tcs = new TaskCompletionSource<ActivityResult>();
                disconnectAudioCallTask = m_tcs.Task;
                m_isExecuteCalled = true;
                this.Run();
            }
            return disconnectAudioCallTask;
        }

        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            ActivityResult activityResult = new ActivityResult(null);
            return activityResult;
        }

        #endregion

    }
}
