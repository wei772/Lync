
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
using System.Configuration;
using System.Configuration.Provider;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{
	public class SqlExpressProfileProvider : ProfileProvider
	{
		#region Fields

		private static string _connectionString;
		private const string _defaultProviderName = "SqlExpressProfileProvider";
        private const string _defaultProdviderDescription = "SQL Express Profile Provider";

		#endregion

		#region Properties

		public override string ApplicationName { get; set; }

		public override string Description
		{
			get
			{
				return _description;
			}
		}
		private string _description;

		public override string Name
		{
			get
			{
				return _name;
			}
		}
		private string _name;

		#endregion

		#region Methods

		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (String.IsNullOrEmpty(name))
				name = _defaultProviderName;
			_name = name;

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", _defaultProdviderDescription);
			}
            _description = GetConfigValue(config["description"], _defaultProdviderDescription);

			ApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			
			ConnectionStringSettings ConnectionStringSettings = ConfigurationManager.ConnectionStrings[config["connectionStringName"]];
			if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
			{
				throw new ProviderException("Connection string cannot be blank.");
			}
			_connectionString = ConnectionStringSettings.ConnectionString;
		}

		public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException();
		}
		
		public override int DeleteProfiles(string[] usernames)
		{
			throw new NotImplementedException();
		}

		public override int DeleteProfiles(ProfileInfoCollection profiles)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection collection)
		{
			string userName = (string)context["UserName"];
			bool isAnonymous = !(bool)context["IsAuthenticated"];

			SettingsPropertyValueCollection propertyValues = new SettingsPropertyValueCollection();
			using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				bool retry = true;
				int retryCount = 0;
				while (retry && retryCount < 2)
				{
					retryCount++;
					var profile = (from p in dataStore.Profile
								   join d in dataStore.ProfileData on p.Id equals d.ProfileId
								   where p.ApplicationName == ApplicationName && p.Username == userName
								   select new
								   {
									   Profile = p,
									   Data = d
								   }).FirstOrDefault();

					if (profile == null)
					{
						retry = true;
						CreateProfile(dataStore, userName);
						dataStore.SubmitChanges();
					}
					else
					{
						retry = false;
						foreach (var property in collection)
						{
							SettingsProperty settingsProperty = property as SettingsProperty;
							SettingsPropertyValue propertyValue = new SettingsPropertyValue(settingsProperty);
							switch (settingsProperty.Name)
							{
								case "FirstName":
									propertyValue.PropertyValue = profile.Data.FirstName;
									break;
								case "LastName":
									propertyValue.PropertyValue = profile.Data.LastName;
									break;
								case "Phone":
									propertyValue.PropertyValue = profile.Data.Phone;
									break;
							}
							propertyValues.Add(propertyValue);

							profile.Profile.LastActivity = DateTime.Now;
						}
						dataStore.SubmitChanges();
					}
				}
			}

			return propertyValues;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="collection"></param>
		public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection collection)
		{
			string userName = (string)context["UserName"];
			bool isAnonymous = !(bool)context["IsAuthenticated"];

			SettingsPropertyValueCollection propertyValues = new SettingsPropertyValueCollection();
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				bool retry = true;
				int retryCount = 0;
				while (retry && retryCount < 2 )
				{
					retryCount++;
					var profile = (from p in dataStore.Profile
								   join d in dataStore.ProfileData on p.Id equals d.ProfileId
								   where p.ApplicationName == ApplicationName && p.Username == userName
								   select new
								   {
									   Profile = p,
									   Data = d
								   }).FirstOrDefault();

					if (profile == null)
					{
						retry = true;
						CreateProfile(dataStore, userName);
						dataStore.SubmitChanges();
					}
					else
					{
						retry = false;
						foreach (var property in collection)
						{
							SettingsPropertyValue settingsPropertyValue = property as SettingsPropertyValue;
							switch (settingsPropertyValue.Property.Name)
							{
								case "FirstName":
									profile.Data.FirstName = (string)settingsPropertyValue.PropertyValue;
									break;
								case "LastName":
									profile.Data.LastName = (string)settingsPropertyValue.PropertyValue;
									break;
								case "Phone":
									profile.Data.Phone = (string)settingsPropertyValue.PropertyValue;
									break;
							}
						}
						profile.Profile.LastUpdated = DateTime.Now;
						profile.Profile.LastActivity = DateTime.Now;
						dataStore.SubmitChanges();
					}
				}
			}
		}

        private void CreateProfile(ProductStore dataStore, string userName)
		{
			var newProfile = new Profile()
			{
				Id = Guid.NewGuid(),
				ApplicationName = ApplicationName,
				Username = userName,
				IsAnonymous = 0
			};
			dataStore.Profile.InsertOnSubmit(newProfile);

			var newProfileData = new ProfileData()
			{
				Id = Guid.NewGuid(),
				ProfileId = newProfile.Id
			};
			dataStore.ProfileData.InsertOnSubmit(newProfileData);
		}

		/// <summary>
		/// A helper function to retrieve config values from the configuration file.
		/// </summary>
		/// <param name="configValue"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private string GetConfigValue(string configValue, string defaultValue)
		{
			if (String.IsNullOrEmpty(configValue))
				return defaultValue;

			return configValue;
		}

		#endregion
	}
}