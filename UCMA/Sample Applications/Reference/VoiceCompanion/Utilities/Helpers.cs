/*=====================================================================
  File:      Helpers.cs

  Summary:   Implements several helper methods.

***********************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
***********************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Rtc.Signaling;
using Microsoft.Rtc.Collaboration.AudioVideo;
using Microsoft.Rtc.Collaboration;
using System.Globalization;
using Microsoft.Rtc.Collaboration.Presence;
using VoiceCompanion.SimpleStatementDialog;
using Microsoft.Rtc.Collaboration.Samples.Common.Activities;
using Microsoft.Rtc.Collaboration.Samples.Common.Dialog;

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion.Utilities
{
    class Helpers
    {
        public static void DetachFlowFromAllDevices(AudioVideoCall call)
        {
            if (call == null || call.Flow == null)
            {
                return;
            }

            var flow = call.Flow;
            if (flow.SpeechRecognitionConnector != null)
            {
                flow.SpeechRecognitionConnector.DetachFlow();
            }

            if (flow.SpeechSynthesisConnector != null)
            {
                flow.SpeechSynthesisConnector.DetachFlow();
            }

            if (flow.ToneController != null)
            {
                flow.ToneController.DetachFlow();
            }
        }

        public static void PlayCallWasDeclined(
            AudioVideoCall avCall,           
            EventHandler<DialogCompletedEventArgs> dialogCompleted)
        {
            try
            {

                SimpleStatementDialog simpleStatDialog = new SimpleStatementDialog("The call was declined.", avCall);
                simpleStatDialog.Completed += dialogCompleted;
                simpleStatDialog.Run();                
            }
            catch (InvalidOperationException)
            {
                // Ignore if we get this since we are just missing an announcement.
            }
        }

        public static bool TryExtractCleanPhone(string uri, out string phone)
        {
            bool result = false;

            phone = null;

            try
            {
                RealTimeAddress address = new RealTimeAddress(uri);
                if (address.IsPhone)
                {
                    phone = address.UserAtHost;
                    string[] parts = phone.Split('@');
                    phone = parts[0]; // Just the user part.
                    result = true;
                }
            }
            catch (ArgumentException)
            {
            }

            return result;
        }

    }
}

namespace Microsoft.Rtc.Collaboration.Samples.VoiceCompanion
{
    /// <summary>
    /// Represents arguments (up to 3) that can be used to pass around classes and methods for convenience. Avoids collection.
    /// </summary>
    /// <remarks>Use this for task methods that need more than one argument.</remarks>
    public class ArgumentTuple
    {
        private object m_one;
        private object m_two;
        private object m_three;

        public ArgumentTuple(object one, object two)
        {
            m_one = one;
            m_two = two;
        }

        public ArgumentTuple(object one, object two, object three)
        {
            m_one = one;
            m_two = two;
            m_three = three;
        }

        public object One
        {
            get
            {
                return m_one;
            }
        }

        public object Two
        {
            get
            {
                return m_two;
            }
        }
        public object Three
        {
            get
            {
                return m_three;
            }
        }
    }
}

