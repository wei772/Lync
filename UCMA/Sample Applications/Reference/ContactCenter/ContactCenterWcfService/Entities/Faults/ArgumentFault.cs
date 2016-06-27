/*=====================================================================
  File:      ArgumentFault.cs
 
  Summary:   Represents input arguments related faults.
 
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
    /// Represents fault details for faults related to input arguments.
    /// </summary>
    [DataContract]
    public class ArgumentFault : OperationFault
    {

        #region private variables

        /// <summary>
        /// Param name.
        /// </summary>
        private string m_paramName = string.Empty;
        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the arugment name that triggered this fault.
        /// </summary>
        [DataMember]
        public string ParamName 
        {
            get { return m_paramName; }
            set
            {
                if (value != null)
                {
                    m_paramName = value;
                }
            }
        }

        #endregion
    }
}
