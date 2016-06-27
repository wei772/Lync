/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

namespace FastHelp
{
    using System;
    using System.ServiceModel.Activation;
    using System.Web;
    using System.Web.Routing;

    public class Global : System.Web.HttpApplication
    {

        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private static void RegisterRoutes()
        {
            // Edit the base address of FastHelpRestService  by replacing the "FastHelpRestService" string below
            RouteTable.Routes.Add(new ServiceRoute("FastHelpRestService", new WebServiceHostFactory(), typeof(FastHelpRestService)));
        }
    }
}