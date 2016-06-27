/*=====================================================================
  File:      ViewModelBase.cs

  Summary:   Base class for view models

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
using System.ComponentModel;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    public interface IViewModel : INotifyPropertyChanged
    {
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Properties

        public static bool IsDesignTime
        {
            get
            {
                return DesignerProperties.IsInDesignTool;
            }
        }

        #endregion

        #region Constructors

        public ViewModelBase()
        {
            if (IsDesignTime)
            {
                OnDesignTime();
            }
            OnInitializeCommands();
        }

        #endregion

        #region Methods

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        protected virtual void NotifyPropertyChanged(Enum property)
        {
            NotifyPropertyChanged(property.ToString());
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                handlers(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected T PropertyFromName<T>( string propertyName)
        {
            return (T)Enum.Parse(typeof(T), propertyName, true);
        }

        protected virtual void OnDesignTime()
        {
        }

        protected virtual void OnInitializeCommands()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
