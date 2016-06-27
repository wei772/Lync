/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.Helpers
{
	public class UpdateBindingOnKeyUp : Behavior<TextBox>
	{
		public UpdateBindingOnKeyUp()
		{
			
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.KeyUp += AssociatedObject_KeyUp;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			AssociatedObject.KeyUp -= AssociatedObject_KeyUp;
		}

		void AssociatedObject_KeyUp(object sender, KeyEventArgs e)
		{
			BindingExpression binding = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
			if (binding != null)
			{
				binding.UpdateSource();
			}
		}
	}
}