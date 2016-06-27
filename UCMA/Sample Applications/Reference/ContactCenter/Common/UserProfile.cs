/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;
using System.Web.Security;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// Adapted form http://weblogs.asp.net/jgalloway/archive/2008/01/19/writing-a-custom-asp-net-profile-class.aspx
	/// </remarks>
	public class UserProfile : ProfileBase
	{
		#region Properties

		[SettingsAllowAnonymous(false)]
		public String FirstName
		{
			get { return base["FirstName"] as String; }
			set { base["FirstName"] = value; }
		}

		[SettingsAllowAnonymous(false)]
		public String LastName
		{
			get { return base["LastName"] as String; }
			set { base["LastName"] = value; }
		}

		[SettingsAllowAnonymous(false)]
		public String Phone
		{
			get { return base["Phone"] as String; }
			set { base["Phone"] = value; }
		}

		#endregion

		#region Methods

		public static UserProfile GetUserProfile(string username)
		{
			return Create(username) as UserProfile;
		}

		public static UserProfile GetUserProfile()
		{
			MembershipUser user = Membership.GetUser();
			if (user != null)
			{
				return Create(user.UserName) as UserProfile;
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}