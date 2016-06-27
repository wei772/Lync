/*=====================================================================
  File:      ComponentBase.cs

  Summary:   Provides an abstract component that could be started and shut down.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.Samples.Utilities;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    public abstract class ComponentBase
    {
        #region Private fields

        private List<AsyncResult> m_pendingShutdownOperations = new List<AsyncResult>();
        private readonly object m_syncRoot = new object();
        private State m_state;
        private AsyncResult m_pendingStartupAsyncResult;
        private AsyncResult m_pendingShutdownAsyncResult;
        private AppPlatform m_appPlatform;
        private string m_name;
        private enum State
        {
            Idle =0,
            Starting = 1,
            Started,
            Terminating,
            Terminated
        }

        #endregion

        #region constructors
        //public ComponentBase()
        //{
        //}

        public ComponentBase(AppPlatform appPlatform)
        {
            m_appPlatform = appPlatform;
        }
        #endregion

        #region public
        /// <summary>
        /// Gets or sets the name of the component. If not set, it is inferred from the type.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (m_name == null)
                {
                    // m_name = this.GetType().Name + "_" + base.GetHashCode().ToString(CultureInfo.InvariantCulture);
                    m_name = this.GetType().Name;
                }
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }
        public virtual void CompleteStartup(Exception exception)
        {

            AsyncResult temp = null;
            lock (this.SyncRoot)
            {
                if (m_pendingStartupAsyncResult == null)
                {
                    return;
                }
                else
                {
                    temp = m_pendingStartupAsyncResult;
                    m_pendingStartupAsyncResult = null;
                }

                if (exception == null)
                {
                    this.SetState(State.Started);
                }
                else
                {
                    this.SetState(State.Idle);
                }
            }

            temp.SetAsCompleted(exception, false);

            this.ResumePendingShutdownsIfRequired();

        }

        public virtual void CompleteShutdown()
        {
            this.CompleteShutdown(null);
        }

        public virtual void CompleteShutdown(Exception exception)
        {
            AsyncResult temp = null;
            lock (this.SyncRoot)
            {
                if (m_pendingShutdownAsyncResult == null)
                {
                    return;
                }
                else
                {
                    this.SetState(State.Terminated);
                    temp = m_pendingShutdownAsyncResult;
                    m_pendingShutdownAsyncResult = null;
                }
            }

            temp.SetAsCompleted(null, false);
            this.ClearPendingShutdowns();
        }

        /// <summary>
        /// Gets the logger module for this component. Can be null.
        /// </summary>
        public virtual Logger Logger
        {
            get
            {
                Logger logger = null;
                if (m_appPlatform != null)
                {
                    logger = m_appPlatform.Logger;
                }
                return logger;
            }
        }
        #endregion

        #region Protected properites

        protected object SyncRoot
        {
            get
            {
                return m_syncRoot;
            }
        }

        public bool IsTerminatingTerminated
        {
            get
            {
                State state = m_state;
                return state == State.Terminating || state == State.Terminated;
            }
        }

        internal AsyncResult PendingStartupAsyncResult
        {
            get
            {
                return m_pendingStartupAsyncResult;
            }
        }

        #endregion

        #region Protected methods
   
        protected abstract void StartupCore();

        protected abstract void ShutdownCore();

        

        #endregion

        #region Internal methods

        internal IAsyncResult BeginStartup(AsyncCallback userCallback, object state)
        {
            var startupAsyncResult
                = new AsyncResult(this,AsyncResult.Operation.Start,userCallback,state);

            this.ProcessStartupAsyncResult(startupAsyncResult);

            return startupAsyncResult;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal void EndStartup(IAsyncResult result)
        {
            ((AsyncResult)result).EndInvoke();
        }
       
        internal IAsyncResult BeginShutdown(AsyncCallback userCallback, object state)
        {
            bool processResult = false;

            lock (m_syncRoot)
            {
                AsyncResult result =
                    new AsyncResult(
                    this,
                    AsyncResult.Operation.Shutdown,
                    userCallback,
                    state);

                switch (m_state)
                {
                    case State.Idle:
                        this.SetState(State.Terminating);
                        processResult = true;
                        break;

                    case State.Starting:
                        m_pendingShutdownOperations.Add(result);
                        break;
                    
                    case State.Started:
                        this.SetState(State.Terminating);
                        processResult = true;
                        break;

                    case State.Terminating:
                        m_pendingShutdownOperations.Add(result);
                        break;

                    case State.Terminated:
                        result.SetAsCompleted(null, true);
                        break;
                }

                if (processResult)
                {
                    result.Process();
                }

                return result;
            }
        }

        internal void EndShutdown(IAsyncResult result)
        {
            ((AsyncResultNoResult)result).EndInvoke();
        }

        internal void ProcessStartupAsyncResult(AsyncResult startupAsyncResult)
        {
            lock (m_syncRoot)
            {                
                //Ensures that only one startup async result is created.
                
                if (m_state != State.Idle)
                {
                    throw new InvalidOperationException("The component is in an invalid state");
                }

                m_state = State.Starting;
            }

            startupAsyncResult.Process();
        }

        #endregion

        #region AsyncResult

        internal class AsyncResult : AsyncResultNoResult
        {
            #region Private fields

            private readonly ComponentBase m_component;
            private readonly Operation m_operation;

            #endregion

            #region Public interface

            public enum Operation
            {
                Start = 0,
                Shutdown
            }

            public AsyncResult(
                ComponentBase component,
                Operation operation,
                AsyncCallback userCallback,
                object state) :
                base(userCallback, state)
            {
                m_component = component;
                m_operation = operation;
            }

            public void Process()
            {
                switch (m_operation)
                {
                    case Operation.Start:
                        m_component.m_pendingStartupAsyncResult = this;
                        m_component.StartupCore();
                        break;

                    case Operation.Shutdown:
                        m_component.m_pendingShutdownAsyncResult = this;
                        m_component.ShutdownCore();
                        break;

                    default:
                        Debug.Assert(false, "Unexpected operation");
                        break;
                }
            }           

        }

        #endregion

        #endregion AsyncResult

        #region Private methods

        private void SetState(State newState)
        {
            lock (m_syncRoot)
            {
                m_state = newState;
            }
        }

        private void ClearPendingShutdowns()
        {
            lock (m_syncRoot)
            {
                foreach (var asyncResult in m_pendingShutdownOperations)
                {
                    asyncResult.SetAsCompleted(null, false);
                }
            }
        }

        private void ResumePendingShutdownsIfRequired()
        {
            lock (m_syncRoot)
            {
                if (m_pendingShutdownOperations.Count > 0 && 
                    (m_state != State.Terminating &&
                     m_state != State.Terminated))
                {
                    var shutDownResult = m_pendingShutdownOperations[0];
                    m_pendingShutdownOperations.RemoveAt(0);

                    this.SetState(State.Terminating);
                    shutDownResult.Process();
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Represence the signature of a task in this application that takes a single parameter.
    /// </summary>
    public delegate void AsyncTaskMethod(AsyncTask ownerTask, object state);

    /// <summary>
    /// Represents a step in the task. A task might consist of several small steps before completion.
    /// </summary>
    public delegate void AsyncTaskStep();


    /// <summary>
    /// Represents the signature of the method that can handle the completion report of an operation.
    /// </summary>
    /// <param name="exception">Exception for the operation. Can be null for successful execution of the operation.</param>
    public delegate void CompletionDelegate(Exception exception);

    /// <summary>
    /// Represents result from a task. Used for advanced cases to chain results across tasks in a sequence.
    /// </summary>
    /// <remarks>A task should subclass to store specific result types. Base class provides only framework.</remarks>
    public class AsyncTaskResult
    {
        private AsyncTaskResult m_previousActionResult; // Result of previous task. Used for chaining results of actions in a sequence.
        private Exception m_exception; // Exception thrown by the current task.

        public AsyncTaskResult(Exception exception)
        {
            m_exception = exception;            
        }

        public AsyncTaskResult(Exception exception, AsyncTaskResult previousActionResult)
        {
            m_exception = exception;
            m_previousActionResult = previousActionResult;
        }

        /// <summary>
        /// Gets or sets the task result for the task that was performed before this task.
        /// </summary>
        public AsyncTaskResult PreviousActionResult
        {
            get
            {
                return m_previousActionResult;
            }
            set
            {
                m_previousActionResult = value;
            }
        }

        /// <summary>
        /// Gets or sets the exception, if any, thorwn by the current task.
        /// </summary>
        public Exception Exception
        {
            get
            {
                return m_exception;
            }
            set
            {
                m_exception = value;
            }
        }
    }

    /// <summary>
    /// Represents the event argument for task completed event.
    /// </summary>
    public class AsyncTaskCompletedEventArgs : EventArgs
    {
        AsyncTaskResult m_actionResult;

        public AsyncTaskCompletedEventArgs(AsyncTaskResult actionResult)
        {
            m_actionResult = actionResult; 
        }

        /// <summary>
        /// Gets the task result of this task.
        /// </summary>
        public AsyncTaskResult ActionResult
        {
            get
            {
                return m_actionResult;
            }
        }
    }

    /// <summary>
    /// Represents a task in this application. 
    /// </summary>
    public class AsyncTask
    {
        // Set to true to turn on logging at base AsyncTask class if task performer logging is insufficient.
        private static bool s_isLoggingOn = false; 

        private AsyncTaskMethod m_workerMethod;
        private object m_state;
        private bool m_isActionDone;
        private bool m_isOptional;
        private AsyncTaskSequence m_actionSequence;
        private AsyncTaskResult m_actionResult;
        private AsyncTaskResult m_previousActionResult;
        private bool m_isCompleted; // Indicates the state of the task.
        private object m_syncRoot = new object(); // For locking purpose.
        private string m_name; // A friendly name for the task. If not set, it is inferred from the delegate.

        /// <summary>
        /// Creates a task with delegate to perform the work for this task.
        /// </summary>
        /// <param name="actionSequence">The sequence that the task belongs to. Can be null if it does not belong to a sequence.</param>
        /// <param name="workerMethod">The task delegate that will perform the steps.</param>
        /// <remarks>The state passed to the method would be null.</remarks>
        public AsyncTask(AsyncTaskMethod workerMethod):this(workerMethod, null)
        {
        }

        /// <summary>
        /// Creates a task with delegate to perform the work for this task.
        /// </summary>
        /// <param name="actionSequence">The sequence that the task belongs to. Can be null if it does not belong to a sequence.</param>
        /// <param name="workerMethod">The task delegate that will perform the steps.</param>
        /// <param name="state">The state to be passed to the method. The method should take the parameter.</param>
        public AsyncTask(AsyncTaskMethod workerMethod, object state)            
        {
            m_workerMethod = workerMethod;
            m_state = state; 
        }

        /// <summary>
        /// Starts a sequence represented by the task given. 
        /// </summary>
        /// <param name="task">The task that contains the sequence.</param>
        /// <remarks>This method expects one argument that should be a sequence. Else, it will complete with failure.</remarks>
        /// <remarks>Note: If actionsequence is null, there will be no logging for this task.</remarks>
        public static void SequenceStartingMethod(AsyncTask task, object state)
        {
            AsyncTaskSequence actionSequence =state as AsyncTaskSequence;
            if (actionSequence == null)
            {
                task.Complete(new InvalidOperationException("Invalid use of AsyncTask.SequenceStartingMethod method. Expecting actionsequence as state."));
                return;
            }
            actionSequence.Start(); // Task will complete when the sequence completes. The report delegate for the sequence should be Complete methods in this task so that this task can complete.
        }

        /// <summary>
        /// Gets the state to be passed to the worker method for its use.
        /// </summary>
        public object State
        {
            get
            {
                return m_state;
            }
        }

        /// <summary>
        /// Invokes the method to do the steps represented by this task. This is called by the owner of the task to start the operation.
        /// </summary>
        public void StartTask()
        {
            if (!m_isActionDone)
            {
                if (s_isLoggingOn && this.Logger != null)
                {
                    this.Logger.Log(Logger.LogLevel.Info, "Starting <" + this.Name + ">");
                }
                m_workerMethod(this, m_state);
                m_isActionDone = true;
            }
        }

        /// <summary>
        /// Performs a step in the task. The completion of this step DOES NOT complete the task. More steps are needed.
        /// </summary>
        /// <param name="step">The step to perform.</param>
        /// <remarks>If the step results in exception, the task is completed with that exception.</remarks>
        public void DoOneStep(AsyncTaskStep step)
        {
            this.DoStepCore(step, false);
        }

        /// <summary>
        /// Performs the final step in the task. The completion of this step WILL complete the task. 
        /// </summary>
        /// <param name="step">The final step to perform.</param>
        /// <remarks>If the step results in exception, the task is completed with that exception. Otherwise, it is completed without exception.</remarks>
        public void DoFinalStep(AsyncTaskStep step)
        {
            this.DoStepCore(step, true);
        }

        /// <summary>
        /// Do a step in the task. The parameter isFinal controls the completion of the task.
        /// </summary>
        /// <param name="step">The step to perform.</param>
        /// <remarks>If the step results in exception, the task is completed with that exception.</remarks>
        protected void DoStepCore(AsyncTaskStep step, bool isFinal)
        {
            Exception exception = null;
            AsyncTaskResult result = null;
            try
            {
                step();
                result = this.TaskResult; // If task stored it, use it.
                if (isFinal)
                {
                    this.Complete(null, result);
                }
            }
            catch (ArgumentException argexp)
            {
                exception = argexp;
            }
            catch (InvalidOperationException ioe)
            {
                exception = ioe;
            }
            catch (RealTimeException rte)
            {
                exception = rte;
            }
            catch (VoiceCompanionException vce)
            {
                exception = vce;
            }
            finally
            {
                if (result == null)
                {
                    result = new AsyncTaskResult(exception);
                }
                if (exception != null)
                {
                    result.Exception = exception;
                    this.Complete(exception, result);
                }
            }
        }

        public Logger Logger
        {
            get
            {
                Logger logger = null;
                if (this.TaskSequence != null)
                {
                    return this.TaskSequence.Logger;
                }
                return logger;
            }
        }

        /// <summary>
        /// Gets the value that indicates if this task is already complete or not.
        /// </summary>
        public bool IsCompleted
        {
            get
            {
                return m_isCompleted;
            }
        }

        /// <summary>
        /// Gets or sets the name of the task. Default is based on the delegate method passed.
        /// </summary>
        public string Name
        {
            get
            {
                if (m_name == null)
                {
                    // m_name =  m_action.Method.Name + "_" + base.GetHashCode().ToString(CultureInfo.InvariantCulture);
                    m_name = m_workerMethod.Method.Name;
                }
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        /// <summary>
        /// Gets or sets the task result from the previous task, if any. Can be null. Is available before the task is performed.
        /// </summary>
        /// <remarks>
        /// Task can use this to make its decisions. It can walk the chain and find the result it is interested in as well.
        ///
        /// The task should take this and chain with its own task result at the end.
        /// If task misses this, task sequence will do it for the task.
        /// </remarks>
        public AsyncTaskResult PreviousActionResult
        {
            get
            {
                return m_previousActionResult;
            }
            set
            {
                m_previousActionResult = value;
            }
        }

        /// <summary>
        /// Gets or sets the task result for this task, if any. Can be null. This should be accessed after the task completes.
        /// </summary>
        public AsyncTaskResult TaskResult
        {
            get
            {
                return m_actionResult;
            }
            set
            {
                m_actionResult = value;
            }
        }

        /// <summary>
        /// Gets the exception resulted for this task.
        /// </summary>
        public Exception Exception
        {
            get
            {
                Exception exception = null;
                if (m_actionResult != null)
                {
                    exception = m_actionResult.Exception;
                }
                return exception;
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates if the task is optional.
        /// </summary>
        /// <remarks>Optional tasks should not complete the underlying operation with exception.</remarks>
        public bool IsOptional
        {
            get
            {
                return m_isOptional;
            }
            set
            {
                m_isOptional = value;
            }
        }

        /// <summary>
        /// Gets or sets the task sequence that this task is a member of. Can be null.
        /// </summary>
        public AsyncTaskSequence TaskSequence
        {
            get
            {
                return m_actionSequence;
            }
            set
            {
                m_actionSequence = value;
            }
        }

        /// <summary>
        /// Completes this task. The delegate MUST call this method to complete the task.
        /// </summary>
        /// <param name="exception">Exception that resulted, if any, for the task. Can be null for successful operation.</param>
        /// <remarks>Convenient method to complete task with null or exception. Use the advance method for dealing with return values.</remarks>
        public void Complete(Exception exception)
        {
            this.Complete(exception, null);
        }

        /// <summary>
        /// Completes this task. The delegate MUST call this method to complete the task.
        /// </summary>
        /// <param name="exception">The exception for the task.</param>
        /// <param name="actionResult">The task result for the task. Can include the exception.</param>
        /// <remarks>The signature includes exception since Complete(null) cannot be disambiguated otherwise.
        /// If the exception given is not null, then it will be stored in the task result as well.
        /// </remarks>
        public void Complete(Exception exception, AsyncTaskResult actionResult)
        {
            lock (m_syncRoot)
            {
                if (m_isCompleted)
                {
                    if (s_isLoggingOn && this.Logger != null)
                    {
                        this.Logger.Log(Logger.LogLevel.Warning, String.Format("Task is completing more than once. Task = {0}", this.Name));
                    }
                    return;
                }
                m_isCompleted = true;
            }
            EventHandler<AsyncTaskCompletedEventArgs> taskCompletedEventHandler = this.TaskCompleted;
            m_actionResult = actionResult;
            if (m_actionResult == null)
            {
                m_actionResult = new AsyncTaskResult(exception);
            }
            if (exception != null)
            {
                m_actionResult.Exception = exception;
            }
            if (taskCompletedEventHandler != null)
            {
                AsyncTaskCompletedEventArgs taskCompletedEventArgs = new AsyncTaskCompletedEventArgs(m_actionResult);
                taskCompletedEventHandler(this, taskCompletedEventArgs);
            }
            if (s_isLoggingOn && this.Logger != null)
            {
                this.Logger.Log(Logger.LogLevel.Info, "Completing <" + this.Name + ">");
            }
        }

        /// <summary>
        /// Rasied when the Task is completed.
        /// </summary>
        public event EventHandler<AsyncTaskCompletedEventArgs> TaskCompleted;
    }

    /// <summary>
    /// Represents a sequence of actions. The actions are done one at a time. The next task is not done until current one is completed.
    /// </summary>
    public abstract class AsyncTaskSequence
    {
        protected Queue<AsyncTask> m_actions;
        private ComponentBase m_component;
        private CompletionDelegate m_successCompletionReportHandlerDelegate;
        private CompletionDelegate m_failureCompletionReportHandlerDelegate;
        private AsyncTask m_pendingAction; // Task that is currently pending.
        private bool m_isComplete; // State of the sequence.
        private object m_syncRoot = new Object(); // for locking.
        private string m_name; // A friendly name for the task sequence. If not set, it is inferred from the type.
        public AsyncTaskSequence(ComponentBase component)
        {
            m_component = component;
            m_actions = new Queue<AsyncTask>(5);
            m_name = String.Empty;
        }

        /// <summary>
        /// Gets or sets the name for the sequence.
        /// </summary>
        public string Name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }
        /// <summary>
        /// Gets the component which is performing this task sequence.
        /// </summary>
        public ComponentBase Component
        {
            get
            {
                return m_component;
            }
        }

        /// <summary>
        /// Gets or sets the handler for the failure completion report of this sequence. Can be null.
        /// </summary>
        /// <remarks>
        /// This handler will be called if any task fails to complete successfully. Otherwise, it will not be called.
        /// </remarks>
        public CompletionDelegate FailureCompletionReportHandlerDelegate
        {
            get
            {
                return m_failureCompletionReportHandlerDelegate;
            }
            set
            {
                m_failureCompletionReportHandlerDelegate = value;
            }
        }

        /// <summary>
        /// Gets or sets the handler for the success completion report of this sequence. Can be null.
        /// </summary>
        /// <remarks>
        /// This handler will be called if all tasks complete successfully. Otherwise, it will not be called.
        /// </remarks>
        public CompletionDelegate SuccessCompletionReportHandlerDelegate
        {
            get
            {
                return m_successCompletionReportHandlerDelegate;
            }
            set
            {
                m_successCompletionReportHandlerDelegate = value;
            }
        }

        /// <summary>
        /// Adds a task to the sequence.
        /// </summary>
        /// <param name="task">The async task to be added to this sequence.</param>
        public void AddTask(AsyncTask task)
        {
            lock (m_actions)
            {
                m_actions.Enqueue(task);
                task.TaskSequence = this;
            }
        }

        /// <summary>
        /// Starts the sequence.
        /// </summary>
        public void Start()
        {
            this.Component.Logger.Log(Logger.LogLevel.Verbose,
                String.Format("**{0}({1})** Started Sequence.", this.Name, this.Component.Name));
            System.Threading.ThreadPool.QueueUserWorkItem(this.Process);
        }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        public Logger Logger
        {
            get
            {
                return Component.Logger;
            }
        }

        /// <summary>
        /// Process Sequence serially.
        /// </summary>
        protected virtual void Process(object state)
        {
            AsyncTaskResult previousActionResult = state as AsyncTaskResult; // Can be null.
            AsyncTask task = null;
            lock (m_actions)
            {
                m_pendingAction = null;
                if (m_actions.Count > 0)
                {
                    task = m_actions.Dequeue();
                    m_pendingAction = task;
                }
            }
            if (task == null)
            {
                // No more items in queue. Complete the starup operaton if needed.
                this.CompleteSequence(null);
            }
            else
            {
                Exception exception = null;
                try
                {
                    task.TaskCompleted +=
                        delegate(object sender, AsyncTaskCompletedEventArgs e)
                        {
                            AsyncTask reportingAction = sender as AsyncTask;
                            if (!Object.ReferenceEquals(reportingAction, task))
                            {
                                this.Logger.Log(Logger.LogLevel.Warning, 
                                        String.Format("Reporting task does not match pending task. Task is probably completing twice. Task = {0}", reportingAction.Name));
                                return; // Ignore this reporting.
                            }
                            if (task.Exception != null && this.Component.Logger != null)
                            {
                                this.Component.Logger.Log(Logger.LogLevel.Verbose, 
                                    String.Format("<{0}> completed with exception.", task.Name), task.Exception);

                            }
                            else
                            {
                                this.Component.Logger.Log(Logger.LogLevel.Verbose,
                                            String.Format("**{0}({1})** <{2}> Completed Task.",
                                            this.Name, this.Component.Name, task.Name));
                            }
                            bool completionNeeded = false;
                            Exception ex = task.Exception;
                            if (task.Exception != null && !task.IsOptional)
                            {
                                completionNeeded = true;
                            }
                            if (completionNeeded)
                            {
                                this.CompleteSequence(ex);
                            }
                            else
                            {
                                // Make sure the task chained previous result, if any.
                                if (previousActionResult != null)
                                {
                                    AsyncTaskResult currActionResult = e.ActionResult;
                                    if (currActionResult == null) // This should not be null if Task always returned it. Play safe.
                                    {
                                        currActionResult = new AsyncTaskResult(task.Exception);
                                    }
                                    // If the task completing has task result, then we need to chain. If not create a new one.
                                    currActionResult.PreviousActionResult = previousActionResult;
                                }
                                // Continue processing.
                                System.Threading.ThreadPool.QueueUserWorkItem(this.Process, e.ActionResult);
                            }
                        };
                    this.Component.Logger.Log(Logger.LogLevel.Verbose, 
                                String.Format("**{0}({1})** <{2}> Started Task.",
                                this.Name, this.Component.Name, task.Name));
                    task.PreviousActionResult = previousActionResult;
                    task.StartTask();
                }
                catch (RealTimeException rte)
                {
                    exception = rte;
                }
                catch (InvalidOperationException ivo)
                {
                    exception = ivo;
                }
                finally
                {
                    if (exception != null && !task.IsOptional)
                    {
                        this.Component.Logger.Log(Logger.LogLevel.Verbose, 
                            String.Format("**{0}({1})** Completing due to exception while starting <{2}>.",
                                          this.Name, this.Component.Name, task.Name), exception);
                        this.CompleteSequence(exception);
                    }
                    else if (exception != null)
                    {
                        this.Component.Logger.Log(Logger.LogLevel.Verbose, String.Format("**{0}({1})** resuming after exception while starting <{2}>.",
                                                   this.Name, this.Component.Name, task.Name), exception);
                        // Continue processing. Ignore failuer in task.
                        System.Threading.ThreadPool.QueueUserWorkItem(this.Process);
                    }
                    // else, wait for the task to complete.
                }
            }
        }

        /// <summary>
        /// Completes the sequence.
        /// </summary>
        /// <param name="exception">The exception to complete with.</param>
        protected void CompleteSequence(Exception exception)
        {
            lock (m_syncRoot)
            {
                if (m_isComplete)
                {
                    this.Component.Logger.Log(Logger.LogLevel.Warning,
                            String.Format("**{0}({1})** Completing operation more than once.",
                            this.Name, this.Component.Name));
                    // It is already complete.
                    return;
                }
                m_isComplete = true;
            }
            this.Component.Logger.Log(Logger.LogLevel.Verbose, 
                    String.Format("**{0}({1})** Completed Sequence.",
                    this.Name, this.Component.Name));
            CompletionDelegate completionCallback;
            if (exception == null)
            {
                completionCallback = this.SuccessCompletionReportHandlerDelegate;
            }
            else
            {
                completionCallback = this.FailureCompletionReportHandlerDelegate;
            }
            if (completionCallback != null)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state) { completionCallback(exception); }, null);                
            }
        }
    }

    /// <summary>
    /// Creates task sequence that are executed serially one by one.
    /// </summary>
    public class AsyncTaskSequenceSerial : AsyncTaskSequence
    {
        public AsyncTaskSequenceSerial(ComponentBase component)
            : base(component)
        {
        }
    }


    /// <summary>
    /// Creates task sequence that are executed in parallel.
    /// </summary>
    /// <remarks>When all actions are complete, the sequence completes. If a task fails and is not optional, the sequence completes.</remarks>
    public class AsyncTaskSequenceParallel : AsyncTaskSequence
    {
        private int m_expectedCount; // # of tasks expected to complete.
        private List<AsyncTask> m_completedActions; // Tasks that have already completed.
        private Exception m_exception; // Final exception to be used for completing the sequence.

        public AsyncTaskSequenceParallel(ComponentBase component)
            : base(component)
        {
            m_completedActions = new List<AsyncTask>();
        }

        /// <summary>
        /// Process actions in parallel.
        /// </summary>
        /// <param name="state"></param>
        protected override void Process(object state)
        {
            lock (m_actions)
            {
                // Init expected count. Lock is not needed since we don't expect app to add task while executing sequence but there is no harm.
                m_expectedCount = m_actions.Count;
            }
            bool done = false;
            while (!done)
            {
                AsyncTask task = null;
                lock(m_actions)
                {
                    if (m_actions.Count > 0)
                    {
                        task = m_actions.Dequeue();
                    }
                }
                if (task == null)
                {
                    done = true; // No more actions to execute.
                }
                else
                {
                    Exception exception = null;
                    task.TaskCompleted +=
                            delegate(object sender, AsyncTaskCompletedEventArgs e)
                            {
                                this.Logger.Log(Logger.LogLevel.Info, String.Format("**{0}({1})** <{2}> Started.", this.Name, this.Component.Name, task.Name));
                                this.AddCompletedAction(task); // Could have failed or succeeded. The method should take care of this.
                            };
                    try
                    {
                        this.Logger.Log(Logger.LogLevel.Info, String.Format("**{0}({1})** <{2}> Starting.", this.Name, this.Component.Name, task.Name)); 
                        task.StartTask();
                    }
                    catch (RealTimeException rte)
                    {
                        exception = rte;
                    }
                    catch (InvalidOperationException ivo)
                    {
                        exception = ivo;
                    }
                    finally
                    {
                        if (exception != null)
                        {
                            this.Logger.Log(Logger.LogLevel.Info, String.Format("**{0}({1})** Completed <{2}> with exception. {3}", 
                                this.Name, this.Component.Name, task.Name, exception.ToString()));
                            this.AddCompletedAction(task); // This task resulted in exception. 
                        }
                    }
                }
            }
            if (m_expectedCount == 0)
            {
                // It was empty list of actions to start with. 
                this.CompleteSequence(null);
            }
        }

        /// <summary>
        /// Adds a completed task to the list of completed tasks.
        /// </summary>
        /// <param name="task">The task to be added.</param>
        private void AddCompletedAction(AsyncTask task)
        {
            Debug.Assert(task.IsCompleted);
            int completedCount = 0;
            lock (m_completedActions)
            {
                m_completedActions.Add(task);
                completedCount = m_completedActions.Count;
            }
            if (!task.IsOptional)
            {
                m_exception = task.Exception; // Last one wins. Either we complete as soon as a non-optional one fails or when all succeed.
            }
            bool completeNow = (completedCount == m_expectedCount);

            if (task.Exception != null && !task.IsOptional)
            {
                completeNow = true;
            }
            if (completeNow)
            {
                this.CompleteSequence(m_exception);
            }
        }

    }
}
