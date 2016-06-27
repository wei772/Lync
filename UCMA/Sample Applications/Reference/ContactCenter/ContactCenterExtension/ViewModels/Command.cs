/*=====================================================================
  File:      Command.cs

  Summary:   Class for handling commands from Views. 

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
using System.Windows.Input;

namespace Microsoft.Lync.Samples.ContactCenterExtension.ViewModels
{
    #region Delegates
    public delegate bool CanExecuteDelegate(Object argument);
    public delegate void ExecuteDelegate(Object argument);
    #endregion

    public class Command : ICommand
    {
        #region Fields

        private EventHandler _canExecuteChanged;

        #endregion

        #region Properties

        public Func<Object, Boolean> CanExecute { get; set; }

        public Action<Object> Execute { get; set; }

        #endregion

        #region Constructors

        public Command()
        {
        }

        public Command(Action<Object> execute)
            : this(execute, null)
        {
        }

        public Command(Action<Object> execute, Func<Object,Boolean> canExecute)
        {
            Execute = execute;
            CanExecute = canExecute;
        }

        #endregion

        #region Methods

        public void NotifyCanExecuteChanged()
        {
            if (_canExecuteChanged != null)
                _canExecuteChanged(this, new EventArgs());
        }

        #endregion

        #region ICommand Members

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute == null || CanExecute(parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { _canExecuteChanged += value; }
            remove { _canExecuteChanged -= value; }
        }

        void ICommand.Execute(object parameter)
        {
            if (Execute != null && (CanExecute == null || CanExecute(parameter)))
            {
                Execute(parameter);
            }
        }

        #endregion
    }
}