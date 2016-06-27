/*=====================================================================
  File:      OperationFault.cs
 
  Summary:   Represents a generic operation fault.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System;
using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{

    /// <summary>
    /// Represents a generic operation fault.
    /// </summary>
    [DataContract]
    [KnownType(typeof(ArgumentFault))]
    public class OperationFault
    {

        #region private variables

        /// <summary>
        /// Fault message.
        /// </summary>
        private string m_message = string.Empty;

        /// <summary>
        /// Inner exception message.
        /// </summary>
        private string m_innerExceptionMessage = string.Empty;

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the fault message.
        /// </summary>
        [DataMember]
        public string Message 
        {
            get { return m_message;  }
            set
            {
                if (value != null)
                {
                    m_message = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the inner exception message, if any.
        /// </summary>
        [DataMember]
        public string InnerExceptionMessage 
        {
            get { return m_innerExceptionMessage; }
            set
            {
                if (value != null)
                {
                    m_innerExceptionMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the source of the fault.
        /// </summary>
        [DataMember]
        public FaultSource Source { get; set; }

        #endregion
    }
}
