/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Text;
    using System.Xml.Linq;
    using System.IO;
    using System.Web; 


    [ServiceContract]
   
    public interface IFastHelpRestService
    {
        [OperationContract]    
        [WebGet(UriTemplate = "IVROptions")]
        XElement MenuOptionsAsXml();
      
    }
}
