/*=====================================================================

  File:      Activity.cs

  Summary:  This file have classes related to activity:- ActivityBase,ActivityResult,
 *                                  related to activity events:- ActivityCompletedEventWrapper,ActivityStateChangedArgs,ActivityCompletedEventArgs
 *                                  related to exceptions of activity:- SilenceTimeOutException, NoRecognitionException
 *                    and Enums
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/
namespace Microsoft.Rtc.Collaboration.Samples.Common.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;




    /// <summary>
    /// Captures the result of any activiy when it completes.
    /// </summary>
    public sealed class ActivityResult
    {
        /// <summary>
        /// Initializes a new instance of ActivityResult class. 
        /// </summary>
        /// <param name="exception">Exception occured in activity.</param>  
        /// <param name="output">Output dictionary contains output results of activity.</param>
        public ActivityResult( Dictionary<string, object> output)
        {
            this.Output = output;
        }

        /// <summary>
        /// Gets or sets Ouptput of the activity.
        /// </summary>
        public Dictionary<string, object> Output { get; set; }
    }

    /// <summary>
    /// Base class for all activities.
    /// </summary>
    public abstract class ActivityBase
    {
       

        /// <summary>
        /// Constructor for this class. Registers an eventhadler for Queue changed.
        /// </summary>
        public ActivityBase()
        {
           
        }
       
        /// <summary>
        /// Initialize Activity properties
        /// </summary>
        /// <param name="parameters">Input parameters in dictionary</param>
        public virtual void InitializeParameters(Dictionary<string, object> parameters)
        {
            return;
        }

    
        /// <summary>
        /// Starts an activity asynchronously.
        /// </summary>
        public abstract Task<ActivityResult> ExecuteAsync();
       

    }


    /// <summary>
    /// enums for call communication states.
    /// </summary>
    internal enum AvCallCommunicationEvents
    {
        None,
        Disconnected,
        Retrieved,
        OnHold
    }

}
