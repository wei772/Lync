
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
using System.Configuration;
using System.Data.Common;
using System.Web.UI.HtmlControls;
using System.Web.Security;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web
{
	public partial class AccountOverview : System.Web.UI.Page
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

			FirstNameLabel.InnerText = profile.FirstName;
			LastNameLabel.InnerText = profile.LastName;
			PhoneLabel.InnerText = profile.Phone;
			EmailLabel.InnerText = user.Email;

            //Hidden text boxes to pass data into chat window
            txtUserName.Text = profile.FirstName + " " + profile.LastName;            
            txtUserPhone.Text = profile.Phone;

		}

		void AgentRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			HtmlButton button = e.Item.FindControl("ChatNowButton") as HtmlButton;
			button.Attributes["sip-address"] = ((Agent) e.Item.DataItem).SipAddress;
		}
	}
}