/*=====================================================================
  File:      FaultSource.cs
 
  Summary:   Source of the fault.
 
 ********************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/


using System.Runtime.Serialization;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Entities
{
    /// <summary>
    /// Represents the source of the fault.
    /// </summary>
    [DataContract]
    public enum FaultSource
    {
        /// <summary>
        /// Indicates that the fault occured due to client inputs. Operation may be retried at a later time or with different inputs.
        /// </summary>
        [EnumMember]
        Client,

        /// <summary>
        /// Indicates that the fault occured due to server side processing of client request.
        /// </summary>
        [EnumMember]
        Server,
    }
}
