/*=====================================================================
  File:      SkillViewModel.cs

  Summary:   View Model for displaying skills for escalation.

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
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    /// <summary>
    /// Skill View-Model Contract
    /// </summary>
    public interface ISkillViewModel
    {
        String DisplayName { get; }

        String Category { get; }

        Boolean HasSelectedTopic { get; }

        ITopicViewModel SelectedTopic { get; }

        List<ITopicViewModel> Topics { get; }

        ICommand ClearSelectedTopicCommand { get; }
    }

    public class SkillViewModel : ViewModel<Skill>, ISkillViewModel
    {
        #region Nested Types

        public enum Properties
        {
            DisplayName,
            SelectedTopic,
            HasSelectedTopic
        }

        #endregion

        #region Constructors

        public SkillViewModel(Skill model)
            :base(model)
        {
            Topics = model.Values.Select(v => new TopicViewModel(this, v) as ITopicViewModel).ToList();

            foreach (var topic in Topics)
            {
                topic.PropertyChanged += TopicPropertyChanged;
            }

            ClearSelectedTopicCommand = new Command
                                            {
                CanExecute = o => HasSelectedTopic,
                Execute = o =>
                {
                    foreach (var topic in Topics) topic.IsSelected = false;
                }
            };
        }

        #endregion

        #region Event Handlers

        void TopicPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (PropertyFromName<TopicViewModel.Properties>(e.PropertyName))
            {
                case TopicViewModel.Properties.IsSelected:
                    NotifyPropertyChanged(Properties.HasSelectedTopic);
                    NotifyPropertyChanged(Properties.SelectedTopic);
                    NotifyPropertyChanged(Properties.DisplayName);
                    ((Command)ClearSelectedTopicCommand).NotifyCanExecuteChanged();
                    break;
            }
        }

        #endregion

        #region ISkillViewModel Members

        public string DisplayName
        {
            get
            {
                if (HasSelectedTopic)
                {
                    return Model.Name + " - " + SelectedTopic.DisplayName;
                }
                return Model.Name;
            }
        }

        public string Category
        {
            get { return Model.Name; }
        }

        public bool HasSelectedTopic
        {
            get { return (SelectedTopic != null); }
        }

        public List<ITopicViewModel> Topics { get; private set; }

        public ITopicViewModel SelectedTopic
        {
            get
            {
                return Topics.FirstOrDefault(t => t.IsSelected);
            }
        }

        public ICommand ClearSelectedTopicCommand { get; private set; }

        #endregion
    }
        
}
