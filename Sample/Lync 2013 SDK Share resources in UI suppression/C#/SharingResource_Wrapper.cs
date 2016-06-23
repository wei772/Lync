/*===================================================================== 
  This file is part of the Microsoft Office 2013 Lync Code Samples. 

  Copyright (C) 2013 Microsoft Corporation.  All rights reserved. 

This source code is intended only as a supplement to Microsoft 
Development Tools and/or on-line documentation.  See these other 
materials for detailed information regarding Microsoft code samples. 

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
PARTICULAR PURPOSE. 
=====================================================================*/

using System;
using Microsoft.Lync.Model.Conversation.Sharing;

namespace ShareResources
{
    class SharingResource_Wrapper
    {
        SharingResource _SharingResource;
        internal Int32 ResourceId
        {
            get
            {
                return _SharingResource.Id;
            }
        }
        public override string ToString()
        {
            return _SharingResource.Name;
        }
        internal SharingResource_Wrapper(SharingResource sharingResource)
        {
            if (sharingResource == null)
            {
                throw new ArgumentNullException();
            }
            _SharingResource = sharingResource;
        }
    }
}
