/*=====================================================================

  File   :  WhileActivity.cs

  Summary:  Implements while activity and exceutes a function registered as even handler to the event ExecuteCondition which returns boolean result.   
 
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
    /// Activity executes a condition of while loop.
    /// </summary>
    public class WhileActivity : ActivityBase
    {

        /// <summary>
        /// This event is responsible for execution of condition of while. It is started when while activity starts its execution.
        /// </summary>
        public event EventHandler<ConditionalEventArgs> ExecuteCondition;


        /// <summary>
        /// Gets or sets result of while condition.
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets output of the activity.
        /// </summary>
        public Dictionary<string, object> Output { get; set; }

        /// <summary>
        /// Taskcompletionsource used to create task for the activity. Task can be set as completed once activity completes its functionality.
        /// </summary>
        private TaskCompletionSource<ActivityResult> m_tcs;
        private bool m_isExecuteCalled;


        /// <summary>
        /// Initialize a new instance of the WhileActivity class.
        /// </summary>
        public WhileActivity()
        {
            this.Output = new Dictionary<string, object>();
        }

        #region Public Functions

        /// <summary>
        /// Asynchronously starts the activity.
        /// </summary>
        /// <returns></returns>
        public override Task<ActivityResult> ExecuteAsync()
        {
            m_tcs = new TaskCompletionSource<ActivityResult>();
             Task<ActivityResult> checkWhileCondition=null;

             if (!m_isExecuteCalled)
             {
               checkWhileCondition = m_tcs.Task;
               m_isExecuteCalled = true;
                 this.Run();
             }
            return checkWhileCondition;
        }



        #endregion

        /// <summary>
        /// This method raises an event to execute condition of while loop.
        /// </summary>
        protected void RaiseExecuteConditionHandler(ConditionalEventArgs e)
        {
            EventHandler<ConditionalEventArgs> handler = ExecuteCondition;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Runs the activity
        /// </summary>
        private void Run()
        {

            try
            {
                //Create an instance of conditional event argument.
                ConditionalEventArgs e = new ConditionalEventArgs();
                //Raise event to execute condition.
                this.RaiseExecuteConditionHandler(e);
                //Get the result from conditional event argument instance
                this.Result = e.Result;
                m_tcs.SetResult(this.GetActivityResult());
            }
            catch (Exception exception)
            {
                m_tcs.SetException(exception); 

            }

        }

        /// <summary>
        /// Returns the result of activity.
        /// </summary>
        private ActivityResult GetActivityResult()
        {
            this.Output.Add("Result", this.Result);
            ActivityResult activityResult = new ActivityResult(this.Output);
            return activityResult;
        }




    }


    /// <summary>
    /// Captures the result of while condition.
    /// </summary>
    public sealed class ConditionalEventArgs : EventArgs
    {
        // Summary:
        //     Initializes a new instance of the System.Workflow.Activities.ConditionalEventArgs
        //     class.
        public ConditionalEventArgs() { }
        //
        // Summary:
        //     Initializes a new instance of the System.Workflow.Activities.ConditionalEventArgs
        //     class using the result of the condition.
        //
        // Parameters:
        //   result:
        //     The result of the condition.
        public ConditionalEventArgs(bool result) { }

        // Summary:
        //     Gets or sets the result of a System.Workflow.Activities.CodeCondition evaluation.
        //
        // Returns:
        //     true if the result of the condition is true; otherwise, false.
        public bool Result { get; set; }
    }
}
