/*=====================================================================

  File   :  CodeActivity.cs

  Summary:  Code Activity executes the registered handler of ExcecuteCode event  
 
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
    using System.Collections.Generic;

    /// <summary>
    /// Code Activity: Executes the registered handler for the event ExecuteCode.
    /// </summary>
    public class CodeActivity : ActivityBase
    {

        /// <summary>
        /// Event to execute the code when activity starts. 
        /// </summary>
        public event EventHandler ExecuteCode;

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;
        private bool m_isExecuteCalled;

        /// <summary>
        ///  Initialize a new instance of CodeActivity.
        /// </summary>
        public CodeActivity()
        {

        }

        #region Public Functions


        /// <summary>
        /// Initialize Parameters for the activity.
        /// </summary>
        /// <param name="parameters"></param>
        public override void InitializeParameters(Dictionary<string, object> parameters)
        {
            //As no property is to be initialized, so left this function with no code.
        }
        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            Task<ActivityResult> executeCodeTask = null;
            if (!m_isExecuteCalled)
            {
                m_tcs = new TaskCompletionSource<ActivityResult>();
                executeCodeTask = m_tcs.Task;
                m_isExecuteCalled = true;
                this.Run();
            }
            return executeCodeTask;
        }


        #endregion

        /// <summary>
        /// This method raises an event to execute code.
        /// </summary>
        protected void RaiseExecuteConditionHandler()
        {
            EventHandler handler = ExecuteCode;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Run the activity.
        /// </summary>
        private void Run()
        {
            try
            {
                RaiseExecuteConditionHandler();
                m_tcs.SetResult(this.GetActivityResult());
            }
            catch (Exception exception)
            {
                m_tcs.TrySetException(exception);
            }

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
