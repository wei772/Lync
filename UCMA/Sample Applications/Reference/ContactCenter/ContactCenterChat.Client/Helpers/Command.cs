/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System.Windows.Input;
using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient
{
	public class Command : ICommand
	{
		public Command()
		{ }

		public Command(Action<Object> execute, Func<Object, Boolean> canExecute)
		{
			ExecuteAction = execute;
			CanExecuteFunc = canExecute;
		}

        public Dispatcher Dispatcher { get; set; } 

		public Action<object> ExecuteAction { get; set; }

		public Func<Object,Boolean> CanExecuteFunc { get; set; }

		public bool CanExecute(object parameter)
		{
            var canExecuteFunc = this.CanExecuteFunc;
            if (canExecuteFunc != null)
			{
                return canExecuteFunc(parameter) && (ExecuteAction != null);
			}
			else
			{
				return (ExecuteAction != null);
			}
		}

		public void NotifyCanExecuteChanged()
		{
            var canExecuteChanged = this.CanExecuteChanged;
            if (canExecuteChanged != null)
			{
                Dispatcher dispatcher = this.Dispatcher;
                if (dispatcher == null)
                {
                    canExecuteChanged(this, new EventArgs());
                }
                else
                {
                    Action method = () => canExecuteChanged(this, new EventArgs());
                    dispatcher.BeginInvoke(method);
                }
			}
		}

		public event System.EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
            var executeAction = this.ExecuteAction;
            if (executeAction != null)
			{
                Dispatcher dispatcher = this.Dispatcher;
                if (dispatcher == null)
                {
                    executeAction(parameter);
                }
                else
                {
                    dispatcher.BeginInvoke(executeAction, parameter);
                }
			}
		}
	}
}