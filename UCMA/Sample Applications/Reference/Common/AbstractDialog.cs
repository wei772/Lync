/*=====================================================================

  File:      AbstractDialog.cs

  Summary:   Abstract Dialog class with basic functions of a Dialog.
             Also contains events required by a Dialog.
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

namespace Microsoft.Rtc.Collaboration.Samples.Common.Dialog
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

   
    
    /// <summary>
    /// Dialog completed event arguments.
    /// Event arguments will have dictionary of output values and exception
    /// </summary>
    public class DialogCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the DialogCompletedEventArgs class.
        /// </summary>
        /// <param name="output">Output dictionary</param>
        public DialogCompletedEventArgs(Dictionary<string, object> output,Exception exception)
        {
            this.Output = output;
            this.Exception = exception;
        }

        /// <summary>
        /// Gets or sets output of dialog.
        /// </summary>
        public Dictionary<string, object> Output { get; set; }

        /// <summary>
        /// Gets or sets exception generated in dialog execution.
        /// </summary>
        public Exception Exception { get; set; }
        
    }


    /// <summary>
    /// Abstract class for dialogs.
    /// </summary>
    public abstract class DialogBase
    {   

        /// <summary>
        /// Event which will be raised when execution od dialog gets completed.
        /// </summary>
        public event EventHandler<DialogCompletedEventArgs> Completed; 

        /// <summary>
        /// Gets or sets Exception.
        /// </summary>
        protected Exception Exception { get; set; }

        /// <summary>
        /// Virtual funtion which runs the activities in the dialog.
        /// </summary>
        public virtual void Run()
        {
            Action<Task> recursiveBody = null;
            IEnumerator<Task> asyncEnumerator = this.GetActivities();
            recursiveBody =
                delegate(Task previousTask)
                {
                  
                        if (previousTask != null && previousTask.IsFaulted)
                        {
                            if (this.Exception == null) this.Exception = previousTask.Exception;
                            this.RaiseDialogCompleteEvent();
                        }
                        else if (asyncEnumerator.MoveNext())
                        {
                            //if any exception occurs in the dialog execution, raise dioalogcompletion handler.
                            if (this.Exception != null)
                            {
                                this.RaiseDialogCompleteEvent();
                            }
                            else      //else continue to execute next task in loop.
                            {
                                asyncEnumerator.Current.ContinueWith(recursiveBody);
                            }
                        }
                        else    //no more task to execute, hence raise dialog completed event.
                        {
                            this.RaiseDialogCompleteEvent();
                        }
                   
                };
            Task.Factory.StartNew(() => { recursiveBody(null); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        /// <summary>
        /// Get all activities in the dialog using yield.
        /// </summary>
        /// <returns>Enumerator for Tasks</returns>
        public abstract IEnumerator<Task> GetActivities();

        /// <summary>
        /// An abstract method overridden by derived class to intialize input parameters for dialog.
        /// </summary>
        /// <param name="inputParameters"></param>
        public abstract void InitializeParameters(Dictionary<string, object> inputParameters);

        /// <summary>
        /// This method raises an event of dialog completed.
        /// </summary>
        protected virtual void DialogCompleteHandler(DialogCompletedEventArgs e)
        {
            EventHandler<DialogCompletedEventArgs> handler = Completed;
            if (handler != null)
                handler(this, e);            
        }

        /// <summary>
        /// An abtstract method overridden by derived dialog classes to raise dialog completed event
        /// </summary>
        protected abstract void RaiseDialogCompleteEvent();
    }
}
  