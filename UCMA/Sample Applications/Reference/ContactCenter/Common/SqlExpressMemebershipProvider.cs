
/******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Configuration;
using System.Configuration.Provider;
using System.Configuration;



namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.Common
{
	/// <summary>
	/// A custom membership provider for SqlExpress.
	/// </summary>
	/// <remarks>
	/// Adapted from http://msdn.microsoft.com/en-us/library/6tc47t75.aspx
	/// </remarks>
	public class SqlExpressMembershipProvider : MembershipProvider
	{
		#region Fields

		private string _connectionString;
		private MachineKeySection _machineKey;
		/// <summary>
		/// "SqlExpressMembershipProvider"
		/// </summary>
		private static string _defaultProviderName = "SqlExpressMembershipProvider";

		#endregion

		#region Construcotrs

		static SqlExpressMembershipProvider()
		{
			
		}

		#endregion

		#region Properties

		public override string ApplicationName { get; set; }

		public override bool EnablePasswordReset { get { return _enablePasswordReset; } }
		private bool _enablePasswordReset;

		public override bool EnablePasswordRetrieval { get { return _enablePasswordReset; } }
		private bool _enablePasswordRetrieval;

		public override int MaxInvalidPasswordAttempts { get { return _maxInvalidPasswordAttempts; } }
		private int _maxInvalidPasswordAttempts;

		public override int MinRequiredNonAlphanumericCharacters { get { return _minRequiredNonAlphanumericCharacters; } }
		private int _minRequiredNonAlphanumericCharacters;

		public override int MinRequiredPasswordLength { get { return _minRequiredPasswordLength; } }
		private int _minRequiredPasswordLength;

		public override int PasswordAttemptWindow { get { return _passwordAttemptWindow; } }
		private int _passwordAttemptWindow;

		public override MembershipPasswordFormat PasswordFormat { get { return _passwordFormat; } }
		private MembershipPasswordFormat _passwordFormat;

		public override string PasswordStrengthRegularExpression { get { return _passwordStrengthRegularExpression; } }
		private string _passwordStrengthRegularExpression;

		public override bool RequiresQuestionAndAnswer { get { return _requiresQuestionAndAnswer; } }
		private bool _requiresQuestionAndAnswer;

		public override bool RequiresUniqueEmail { get { return _requiresUniqueEmail; } }
		private bool _requiresUniqueEmail;

		#endregion

		#region Methods

		/// <summary>
		/// Initializes the provider.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config"></param>
		public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			if (String.IsNullOrEmpty(name))
				name = _defaultProviderName;

			if (String.IsNullOrEmpty(config["description"]))
			{
				config.Remove("description");
				config.Add("description", "SQL Express Membership provider");
			}

			// Initialize the abstract base class.
			base.Initialize(name, config);

			ApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			_maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
			_passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
			_minRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
			_minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
			_passwordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
			_enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
			_enablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
			_requiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
			_requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
			//WriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

			string temp_format = config["passwordFormat"];
			if (temp_format == null)
			{
				temp_format = "Hashed";
			}

			switch (temp_format)
			{
				case "Hashed":
					_passwordFormat = MembershipPasswordFormat.Hashed;
					break;
				case "Encrypted":
					_passwordFormat = MembershipPasswordFormat.Encrypted;
					break;
				case "Clear":
					_passwordFormat = MembershipPasswordFormat.Clear;
					break;
				default:
					throw new ProviderException("Password format not supported.");
			}

			//
			// Initialize OdbcConnection.
			//

			ConnectionStringSettings ConnectionStringSettings =
			  ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

			if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
			{
				throw new ProviderException("Connection string cannot be blank.");
			}

			_connectionString = ConnectionStringSettings.ConnectionString;


			// Get encryption and decryption key information from the configuration.
			Configuration cfg =
			  WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
			_machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

			if (_machineKey.ValidationKey.Contains("AutoGenerate"))
				if (PasswordFormat != MembershipPasswordFormat.Clear)
					throw new ProviderException("Hashed or Encrypted passwords " +
												"are not supported with auto-generated keys.");
		}

		public override bool ChangePassword(string username, string oldPassword, string newPassword)
		{
			throw new NotImplementedException();
		}

		public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Creates a user.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="email"></param>
		/// <param name="passwordQuestion"></param>
		/// <param name="passwordAnswer"></param>
		/// <param name="isApproved"></param>
		/// <param name="providerUserKey"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
		{
			ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, password, true);
			OnValidatingPassword(args);
			if (args.Cancel)
			{
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (String.IsNullOrEmpty(username))
			{
				status = MembershipCreateStatus.InvalidUserName;
				return null;
			}

			if (String.IsNullOrEmpty(password) || password.Length < MinRequiredPasswordLength)
			{
				status = MembershipCreateStatus.InvalidPassword;
				return null;
			}

			if (RequiresUniqueEmail && !String.IsNullOrEmpty(GetUserNameByEmail(email)))
			{
				status = MembershipCreateStatus.DuplicateEmail;
				return null;
			}

			var existingUser = GetUser(username, false);
			if (existingUser == null)
			{
				if (providerUserKey == null)
				{
					providerUserKey = Guid.NewGuid();
				}

				using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
				{
					User newUser = new User()
					{
						ApplicationName = ApplicationName,
						Id = (Guid)providerUserKey,
						CreationDate = DateTime.Now,
						Email = email,
						FailedPasswordAnswerAttemptCount = 0,
						FailedPasswordAnswerAttemptWindowStart = null,
						FailedPasswordAttemptCount = 0,
						FailedPasswordAttemptWindowStart = null,
						IsApproved = 1,
						IsLockedOut = 0,
						IsOnline = 0,
						LastActivityDate = null,
						LastLockedOutDate = null,
						LastLoginDate = null,
						LastPasswordChangedDate = null,
						Password = password,
						PasswordAnswer = passwordAnswer,
						PasswordQuestion = passwordQuestion,
						Username = username
					};

					MembershipUser member = GetMembershipUserFromData(newUser);
					dataStore.User.InsertOnSubmit(newUser);
					dataStore.SubmitChanges();
					status = MembershipCreateStatus.Success;
					return member;
				}
			}
			else
			{
				status = MembershipCreateStatus.DuplicateUserName;
				return null;
			}
		}

		/// <summary>
		/// Deletes a user.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="deleteAllRelatedData"></param>
		/// <returns></returns>
		public override bool DeleteUser(string username, bool deleteAllRelatedData)
		{
			bool deleted;
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				var user = (from u in dataStore.User
							where u.Username == username && u.ApplicationName == ApplicationName
							select u).FirstOrDefault();
				if (user != null)
				{
					dataStore.User.DeleteOnSubmit(user);
					deleted = true;
				}
				else
				{
					deleted = false;
				}
			}
			return deleted;
		}

		public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
		{
			throw new NotImplementedException();
		}

		public override int GetNumberOfUsersOnline()
		{
			throw new NotImplementedException();
		}

		public override string GetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a user by username.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="userIsOnline"></param>
		/// <returns></returns>
		public override MembershipUser GetUser(string username, bool userIsOnline)
		{
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				var user = (from u in dataStore.User
							where u.Username == username && u.ApplicationName == ApplicationName
							select u).FirstOrDefault();

				if (user != null && userIsOnline)
				{
					user.LastActivityDate = DateTime.Now;
					dataStore.SubmitChanges();
				}

				return GetMembershipUserFromData(user);
			}
		}

		/// <summary>
		/// Gets a user by providerUserKey
		/// </summary>
		/// <param name="providerUserKey"></param>
		/// <param name="userIsOnline"></param>
		/// <returns></returns>
		public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
		{
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				var user = (from u in dataStore.User
							where u.Id == (Guid)providerUserKey && u.ApplicationName == ApplicationName
							select u).FirstOrDefault();

				if (userIsOnline)
				{
					user.LastActivityDate = DateTime.Now;
					dataStore.SubmitChanges();
				}

				return GetMembershipUserFromData(user);
			}
		}

		/// <summary>
		/// Gets the users name by email.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public override string GetUserNameByEmail(string email)
		{
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				var user = (from u in dataStore.User
							where u.Email == email && u.ApplicationName == ApplicationName
							select u).FirstOrDefault();
				if (user != null)
				{
					return user.Username;
				}
				else
				{
					return String.Empty;
				}
			}

		}

		public override string ResetPassword(string username, string answer)
		{
			throw new NotImplementedException();
		}

		public override bool UnlockUser(string userName)
		{
			throw new NotImplementedException();
		}

		public override void UpdateUser(MembershipUser user)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Validates a user loging the user in.
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public override bool ValidateUser(string username, string password)
		{
			bool valid = false;
            using (var dataStore = new ProductStore(SqlHelper.Current.GetConnection(_connectionString)))
			{
				var user = (from u in dataStore.User
							where u.Username == username &&
							u.ApplicationName == ApplicationName &&
							u.IsLockedOut != 1
							select u).FirstOrDefault();
				if ( user!= null && IsPasswordValid(password, user.Password))
				{
					if (user.IsApproved == 1)
					{
						user.LastLoginDate = DateTime.Now;
						valid = true;
						dataStore.SubmitChanges();
					}
				}
				else
				{
					valid = false;
				}
			}
			return valid;
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

		private MembershipUser GetMembershipUserFromData(User user)
		{
			if (user == null)
			{
				return null;
			}
			else
			{
				return new MembershipUser(
					Name,
					user.Username,
					user.Id,
					user.Email,
					user.PasswordQuestion,
					string.Empty /*comment*/,
					(user.IsApproved == 1) ? true : false,
					(user.IsLockedOut == 1) ? true : false,
					user.CreationDate,
					user.LastLoginDate ?? user.CreationDate,
					user.LastActivityDate ?? user.CreationDate,
					user.LastPasswordChangedDate ?? user.CreationDate,
					user.LastLockedOutDate ?? DateTime.MinValue
				);
			}
		}

		private bool IsPasswordValid(string submitedPassword, string userPassword)
		{
			return (submitedPassword == userPassword);
		}

		#endregion
	}
}
