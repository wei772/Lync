/*=====================================================================

File   :  OutBoundInstantMessagingCall.cs

Summary:  Makes an Outbound call to specified sip uri   
 
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
    /// Establishes an instant message call to a destination sip Uri.
    /// </summary>
    public class OutBoundInstantMessagingCall : ActivityBase
    {
        private string m_uri;
        private ConversationSettings m_convSettings;
        private ApplicationEndpoint m_appEndPoint;



        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;

        /// <summary>
        /// Output of the activity.
        /// </summary>
        private Dictionary<string, object> m_output;
        private bool m_isExecuteCalled;
        #region public properties

        /// <summary>
        /// Instant message call to be created.
        /// </summary>
        public InstantMessagingCall InstantMessagingCall { get; set; }


        /// <summary>
        /// Conversation settings.
        /// </summary>
        public ConversationSettings ConvSettings
        {
            get
            {
                return m_convSettings;
            }
            set
            {
                if (value != null)
                    m_convSettings = value;
                else
                    throw new ArgumentNullException("convSettings", "OutBoundInstantMessagingCall");
            }
        }

        /// <summary>
        /// Application end point.
        /// </summary>
        public ApplicationEndpoint AppEndPoint
        {
            get
            {
                return m_appEndPoint;
            }
            set
            {
                if (value != null)
                    m_appEndPoint = value;
                else
                    throw new ArgumentNullException("appEndPoint", "OutBoundInstantMessagingCall");
            }
        }
        /// <summary>
        /// Conversation.
        /// </summary>
        public Conversation Conversation { get; set; }

        /// <summary>
        /// Destination sip uri to connect.
        /// </summary>
        public string DestinationUri
        {
            get
            {
                return m_uri;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    m_uri = value;
                else
                    throw new ArgumentNullException("destinationUri", "OutBoundInstantMessagingCall");
            }
        }

        #endregion






        #region Constructors

        /// <summary>
        /// Initialize a new instance of OutBoundInstantMessagingCall.
        /// </summary>
        private OutBoundInstantMessagingCall()
        {
            m_output = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialize a new instance of OutBoundInstantMessagingCall.
        /// Throws ArgumentNullException if appEndPoint or conversationSettings or destinationUri is null
        /// </summary>
        /// <param name="appEndPoint"></param>
        /// <param name="csettings"></param>
        /// <param name="destinationUri"></param>
        public OutBoundInstantMessagingCall(ApplicationEndpoint appEndPoint, ConversationSettings csettings, string destinationUri)
            : this()
        {
            this.AppEndPoint = appEndPoint;
            this.ConvSettings = csettings;
            this.DestinationUri = destinationUri;
        }

        #endregion

        #region Public Function

        /// <summary>
        /// Initialize Parameters required to establish instant message call.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("ApplicationEndPoint"))
                this.AppEndPoint = parameters["ApplicationEndPoint"] as ApplicationEndpoint;
            if (parameters.ContainsKey("ConversationSettings"))
                this.ConvSettings = parameters["ConversationSettings"] as ConversationSettings;
            if (parameters.ContainsKey("DestinationUri"))
                this.DestinationUri = parameters["DestinationUri"] as string;

        }

        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            m_tcs = new TaskCompletionSource<ActivityResult>();
             Task<ActivityResult> establishImCallTask = m_tcs.Task;

             if (!m_isExecuteCalled)
             {
                 establishImCallTask = m_tcs.Task;
                 m_isExecuteCalled = true;
                 this.Run();
             }
            return establishImCallTask;
        }


        #endregion


       


        /// <summary>
        /// Runs the activity.
        /// </summary>
        private void Run()
        {
            this.Conversation = new Conversation(this.AppEndPoint, this.ConvSettings);
            this.InstantMessagingCall = new InstantMessagingCall(this.Conversation);

            Task.Factory.FromAsync<string, ToastMessage, CallEstablishOptions, CallMessageData>(
                InstantMessagingCall.BeginEstablish,
                InstantMessagingCall.EndEstablish,
                DestinationUri,
                new ToastMessage("Hi"),
                null,
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
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            if (m_output.Count > 0)
                m_output.Clear();
            m_output.Add("Result", this.InstantMessagingCall);
            ActivityResult activityResult = new ActivityResult(m_output);
            return activityResult;
        }

    }
}
