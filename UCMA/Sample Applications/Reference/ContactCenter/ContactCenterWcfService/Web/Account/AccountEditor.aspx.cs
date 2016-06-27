
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;
using System.Configuration;
using System.Web.Security;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web.Account
{
	public partial class AccountEditor : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				DataBindAccount();
			}
		}

		private void DataBindAccount()
		{
			MembershipUser user = Membership.GetUser();
			UserProfile profile = UserProfile.GetUserProfile(user.UserName);

			FirstName.Text = profile.FirstName;
			LastName.Text = profile.LastName;
			Phone.Text = profile.Phone;
			Email.Text = user.Email;
		}

		protected void UpdateUserButton_Click(object sender, EventArgs e)
		{
			MembershipUser user = Membership.GetUser();
			UserProfile profile = UserProfile.GetUserProfile(user.UserName);
			profile.FirstName = FirstName.Text;
			profile.LastName = LastName.Text;
			profile.Phone = Phone.Text;
			user.Email = Email.Text;

			profile.Save();
			Membership.UpdateUser(user);
		}
	}
}