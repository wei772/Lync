/*=====================================================================
  File:      ParticipantViewModel.cs

  Summary:   View Model for participant list of monitored conversation.

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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region Interfaces

    public interface IParticipantViewModel
    {
        String Uri { get; }

        IList<string> MediaTypes { get; }

        String DisplayName { get; }

        Boolean IsCustomer { get; }
    }

    #endregion

    public class ParticipantViewModel : ViewModelBase, IParticipantViewModel
    {
        #region Fields

        private enum Properties
        {
            IsOnVoiceModality,
            IsOnInstantMessagingModality,
            IsOnVideoModality,
            IsOnAppSharingModality
        }

        private ObservableCollection<string> _mediaTypes;

        #endregion Fields

        #region Properties

        public string Uri { get; set; }

        public bool IsCustomer { get; set; }

        public IList<string> MediaTypes 
        {
            get
            {
                if (_mediaTypes == null)
                {
                    _mediaTypes = new ObservableCollection<string>();
                    _mediaTypes.CollectionChanged += (s, e) =>
                    {
                        NotifyPropertyChanged(Properties.IsOnVoiceModality);
                        NotifyPropertyChanged(Properties.IsOnInstantMessagingModality);
                        NotifyPropertyChanged(Properties.IsOnVideoModality);
                        NotifyPropertyChanged(Properties.IsOnAppSharingModality);
                    };
                }
                return _mediaTypes;
            }
        }

        public bool IsOnVoiceModality
        {
            get
            {
                return MediaTypes.Contains("audio");
            }
        }

        public bool IsOnInstantMessagingModality
        {
            get
            {
                return MediaTypes.Contains("message");
            }
        }

        public bool IsOnVideoModality
        {
            get
            {
                return MediaTypes.Contains("video");
            }
        }

        public bool IsOnAppSharingModality
        {
            get
            {
                return MediaTypes.Contains("applicationsharing");
            }
        }

        public String DisplayName 
        { 
            get; 
            set; 
        }

        #endregion Properties

        #region Public Methods

        public override bool Equals(object obj)
        {
            bool match = false;
            ParticipantViewModel avm = obj as ParticipantViewModel;

            if (avm != null && string.Equals(Uri, avm.Uri))
            {
                match = true;
            }

            return match;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        #endregion
    }
        
}