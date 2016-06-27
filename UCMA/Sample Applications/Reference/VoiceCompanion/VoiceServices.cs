/*=====================================================================
  File:      VoiceServices.cs

  Summary:   Implements base classes to define voice services.
 
***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/

using System;
using System.Runtime.Serialization;
using Microsoft.Rtc.Collaboration.AudioVideo;
using System.Diagnostics;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.VoiceServices
{
    #region VoiceServiceFactory

    internal static class VoiceServiceFactory
    {
        #region Private methods and fields

        private static VoiceService CreateType(VoiceServiceInformation info, CustomerSession customerSession)
        {
            VoiceService service = null;

            try
            {
                string fullQualifiedType;

                if (string.IsNullOrEmpty(info.Assembly))
                {
                    fullQualifiedType = info.VoiceServiceType;
                }
                else
                {
                    fullQualifiedType = string.Concat(info, ", ", info.Assembly);
                }

                Type t = Type.GetType(fullQualifiedType, true);
                object o = Activator.CreateInstance(t, customerSession);
                service = (VoiceService)o;

            }
            catch (TypeLoadException e)
            {
                customerSession.Logger.Log(e);
            }

            return service;
        }

        #endregion

        #region Public methods

        public static bool TryGetVoiceServiceInstance(string id, CustomerSession customerSession, out VoiceService voiceService)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            voiceService = null;
            VoiceServiceInformation info = null;
            if(ApplicationConfiguration.TryGetVoiceServiceInformation(id,out info))
            {
                voiceService = CreateType(info, customerSession);
            }

            return (voiceService != null);
        }

        #endregion

    }

    #endregion 

    public abstract class VoiceService:ComponentBase
    {
        #region Public Properties
        public abstract string Id
        {
            get;
        }

        public override Logger Logger
        {
            get
            {
                return this.CustomerSession.Logger;
            }
        }
        #endregion

        #region Constructor

        protected VoiceService(CustomerSession customerSession):base(customerSession.AppFrontEnd.AppPlatform)
        {
            Debug.Assert(customerSession != null);
            if (customerSession == null)
            {
                throw new ArgumentNullException("customerSession");
            }

            this.CustomerSession = customerSession;
        }

        #endregion

        #region Public methods

        public void StartMusic(AudioVideoCall call)
        {
            this.CustomerSession.MusicOnHoldProvider.StartMusic(call);
        }

        public void StopMusic(AudioVideoCall call)
        {
            this.CustomerSession.MusicOnHoldProvider.StopMusic(call);
        }

        #endregion

        #region Protected properties

        public CustomerSession CustomerSession
        {
            get;
            private set;
        }

        #endregion

        #region Protected methods
        public override void CompleteShutdown()
        {
            try
            {
                base.CompleteShutdown();
            }
            finally
            {
                this.CustomerSession.VoiceServiceCompleted(this);
            }
        }
        #endregion
    }
}
