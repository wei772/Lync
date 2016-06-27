/*=====================================================================
  File:      ConversationViewModel.cs

  Summary:   The view model for conversation 

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
using Microsoft.Lync.Model.Conversation;
using System.Collections.ObjectModel;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    public interface IConversationViewModel
    {
        Boolean IsOnHold { get; }
        ObservableCollection<ParticipantViewModel> Customers { get; }
        ObservableCollection<IAgentViewModel> Agents { get; }
    }

    public class ConversationViewModel : ViewModel<Conversation>
    {
        #region Properties

        /// <summary>
        /// Gets or sets whether the conversation is on hold.
        /// </summary>
        public Boolean IsOnHold
        {
            get
            {
                return _isOnHold;
            }
            set
            {
                if (_isOnHold != value)
                {
                    _isOnHold = value;
                    NotifyPropertyChanged(IsOnHoldPropertyName);
                }
            }
        }
        private Boolean _isOnHold;
        public const String IsOnHoldPropertyName = "IsOnHold";

        public ObservableCollection<ParticipantViewModel> Customers { get; set; }

        public ObservableCollection<IAgentViewModel> Agents { get; set; }

        #endregion

        #region Constructors
                
        public ConversationViewModel(Conversation conversation)
            : base(conversation)
        {
        }

        #endregion

    }
        
}
