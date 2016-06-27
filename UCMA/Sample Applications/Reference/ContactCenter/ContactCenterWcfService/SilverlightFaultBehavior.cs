/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService
{

    internal class SilverlightFaultMessageInspector : IDispatchMessageInspector
    {

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply != null && reply.IsFault)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty();

                // Here the response code is changed to 200.
                property.StatusCode = System.Net.HttpStatusCode.OK;


                reply.Properties[HttpResponseMessageProperty.Name] = property;
            }
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // Do nothing to the incoming message.
            return null;
        }
    }


    /// <summary>
    /// Represents silverlight fault behavior.
    /// </summary>
    public class SilverlightFaultBehavior : BehaviorExtensionElement, IEndpointBehavior
    {

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            SilverlightFaultMessageInspector inspector = new SilverlightFaultMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        // The following methods are stubs and not relevant. 

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }


        public void Validate(ServiceEndpoint endpoint)
        { 
        }

        public override System.Type BehaviorType
        {
            get { return typeof(SilverlightFaultBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new SilverlightFaultBehavior();
        }

    }
}
