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

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FastHelpRestService : IFastHelpRestService
    {
        private static XElement xmlContent;

        private XElement GetXmlContents()
        {   
            string apPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            string path = Path.Combine(apPath, "FastHelpIVRMenuXml\\IVRMenu.xml");
            string contents = File.ReadAllText(path);
            XElement xElem = XElement.Parse(contents);
            return xElem;
        }

        public XElement MenuOptionsAsXml()
        {
            if (xmlContent == null)
            {
                xmlContent = GetXmlContents();
            }

            return xmlContent;
        }
    }
}
