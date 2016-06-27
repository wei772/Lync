
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
using System.Web.Security;
using Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenterWcfService.Web
{
	public partial class Login : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

		protected void RegisterUser_CreatedUser(object sender, EventArgs e)
		{
			FormsAuthentication.SetAuthCookie(RegisterUser.UserName, false /* createPersistentCookie */);
			UserProfile.Create(RegisterUser.UserName, true);
			UserProfile profile = UserProfile.GetUserProfile(RegisterUser.UserName);

			profile.FirstName = ((TextBox)RegisterUser.CreateUserStep.ContentTemplateContainer.FindControl("FirstName")).Text;
			profile.LastName = ((TextBox)RegisterUser.CreateUserStep.ContentTemplateContainer.FindControl("LastName")).Text;
			profile.Phone = ((TextBox)RegisterUser.CreateUserStep.ContentTemplateContainer.FindControl("Phone")).Text;

			profile.Save();

			string continueUrl = RegisterUser.ContinueDestinationPageUrl;
			if (String.IsNullOrEmpty(continueUrl))
			{
				continueUrl = "~/";
			}
			Response.Redirect(continueUrl);
			
		}
	}
}