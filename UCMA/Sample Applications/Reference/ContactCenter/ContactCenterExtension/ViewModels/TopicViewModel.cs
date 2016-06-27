/*=====================================================================
  File:      TopicViewModel.cs

  Summary:   View Model for selection of skill topics for escalation.

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

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region Interfaces

    public interface ITopicViewModel : IViewModel
    {
        String DisplayName { get; }
        ISkillViewModel Skill { get; }
        Boolean IsSelected { get; set; }
    }

    #endregion

    public class TopicViewModel : ViewModelBase, ITopicViewModel
    {
        #region Fields

        private Boolean _isSelected;

        #endregion

        #region Properties

        public enum Properties
        {
            IsSelected
        }

        public String DisplayName { get; private set; }
        public ISkillViewModel Skill { get; private set; }
        public Boolean IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged(Properties.IsSelected);
                }
            }
        }

        #endregion

        #region Constructors

        internal TopicViewModel(ISkillViewModel skill, string displayName )
        {
            Skill = skill;
            DisplayName = displayName;
        }

        #endregion
    }


    
}
