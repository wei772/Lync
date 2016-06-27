/*=====================================================================
  File:      AgentViewModel.cs

  Summary:   View Models for views of agents in different contexts.

---------------------------------------------------------------------
This file is part of the Microsoft Lync SDK Code Samples

  Copyright (C) 2010 Microsoft Corporation.  All rights reserved.

This source code is intended only as a supplement to Microsoft
Development Tools and/or on-line documentation.  See these other
materials for detailed information regarding Microsoft code samples.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
=====================================================================*/
using System;
using System.Windows.Threading;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region interfaces

    public interface IAgentViewModel : IParticipantViewModel
    {
        #region Public Abstract Properties

        String Status { get; set; }

        DateTime StartTime { get; set; }

        string Duration { get; }

        Boolean IsActive { get; }

        #endregion
    }
    
    #endregion

    public class AgentViewModel : ParticipantViewModel, IAgentViewModel
    {
        #region Fields

        /// <summary>
        /// Type safe property changed event
        /// </summary>
        public enum Properties
        {
            Status,
            IsActive,
            Duration
        }

        private string _status;

        private readonly DispatcherTimer _durationUpdateTimer;

        #endregion Fields

        #region Constructors

        public AgentViewModel()
        {
            _durationUpdateTimer = new DispatcherTimer();
            _durationUpdateTimer.Tick += (s, e) => NotifyPropertyChanged(Properties.Duration);
            _durationUpdateTimer.Start();
        }

        #endregion

        #region IAgentViewModel Members

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status == value)
                    return;
                _status = value;
                NotifyPropertyChanged(Properties.Status);
                NotifyPropertyChanged(Properties.IsActive);
            }
        }

        public DateTime StartTime { get; set; }

        public string Duration
        {
            get
            {
                TimeSpan ts = DateTime.Now - StartTime;
                return String.Format("{0:0}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public Boolean IsActive
        {
            get
            {
                switch (Status.ToLower())
                {
                    case "idle":
                        return false;
                    case "away":
                        return false;
                    default:
                        return true;
                }
            }
        }

        #endregion

    }
}
