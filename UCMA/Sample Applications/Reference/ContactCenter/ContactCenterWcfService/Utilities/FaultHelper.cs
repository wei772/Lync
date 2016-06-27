/*=====================================================================
  File:      FaultHelper.cs
 
  Summary:   Represents helper methods to construct and parse faults.
 

/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Diagnostics;

using Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Utilities
{
    /// <summary>
    /// Represents helper methods to construct and parse faults.
    /// </summary>
    internal static class FaultHelper
    {
        /// <summary>
        /// Method to create argument fault.
        /// </summary>
        /// <param name="message">Fault message.</param>
        /// <param name="paramName">Parameter that caused the fault.</param>
        /// <returns>Argument fault.</returns>
        internal static ArgumentFault CreateArgumentFault(string message, string paramName)
        {
            Debug.Assert(!String.IsNullOrEmpty(message), "Provide a valid message string");
            Debug.Assert(!String.IsNullOrEmpty(paramName), "Provide a valid param name");

            ArgumentFault argFault = new ArgumentFault();
            argFault.Message = message;
            argFault.ParamName = paramName;
            argFault.Source = FaultSource.Client;

            return argFault;
        }


        /// <summary>
        /// Method to create generic client operation fault.
        /// </summary>
        /// <param name="message">Fault message.</param>
        /// <param name="innerException">Inner exception, if any.</param>
        /// <returns>Argument fault.</returns>
        internal static OperationFault CreateClientOperationFault(string message, Exception innerException)
        {
            return CreateOperationFault(message, innerException, FaultSource.Client);
        }

        /// <summary>
        /// Method to create generic server operation fault.
        /// </summary>
        /// <param name="message">Fault message.</param>
        /// <param name="innerException">Inner exception, if any.</param>
        /// <returns>Argument fault.</returns>
        internal static OperationFault CreateServerOperationFault(string message, Exception innerException)
        {
            return CreateOperationFault(message, innerException, FaultSource.Server);
        }


        /// <summary>
        /// Method to create generic operation fault.
        /// </summary>
        /// <param name="message">Fault message.</param>
        /// <param name="innerException">Inner exception, if any.</param>
        /// <param name="faultSource">Fault source.</param>
        /// <returns>Operation fault.</returns>
        private static OperationFault CreateOperationFault(string message, Exception innerException, FaultSource faultSource)
        {
            Debug.Assert(!String.IsNullOrEmpty(message), "Provide a valid message string");

            OperationFault operationFault = new OperationFault();
            operationFault.Message = message;
            if (innerException != null)
            {
                operationFault.InnerExceptionMessage = innerException.Message;
            }
            operationFault.Source = faultSource;
            return operationFault;
        }
    }
}