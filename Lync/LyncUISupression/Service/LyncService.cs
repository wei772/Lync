using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyncUISupression.Service
{
	public class LyncService
	{

		public delegate void UserIsSignedInDelegate(LyncClient lyncClient);
		//User has signed in to Lync
		public event UserIsSignedInDelegate UserIsSignedIn;


		public delegate void ClientStateChangedDelegate(string newState);
		//The state of the Lync client has changed.
		public event ClientStateChangedDelegate ClientStateChanged;

		/// <summary>
		/// Flag that indicates that this instance of the ShareResources
		/// process initialized Lync. Other instances of ShareResources must not
		/// attempt to shut down Lync
		/// </summary>
		private bool _thisProcessInitializedLync = false;

		/// <summary>
		/// Indicates the user is starting a Side-by-side instance of Lync
		/// </summary>
		private bool _inSideBySideMode = false;

		/// <summary>
		/// Lync client platform. The entry point to the API
		/// </summary>
		private LyncClient _lyncClient;

		private string _userUri;

		private string _userPassword;

		public LyncClient Client
		{
			get
			{
				return _lyncClient;
			}
		}

		public bool ThisProcessInitializedLync
		{
			get
			{
				return _thisProcessInitializedLync;
			}
		}

		private static LyncService _instance = null;
		public static LyncService Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new LyncService();
				}
				return _instance;
			}
		}

		private LyncService()
		{

		}


		/// <summary>
		/// Gets the Lync client, initializes if in UI suppression, and 
		/// starts the user sign in process. This method can raise exceptions
		/// which are thrown if the calling form has not registered a callback for
		/// exception specific events that are declared in this class.
		/// </summary>
		/// <param name="sideBySide">bool. Specifies endpoint mode</param> 
		public void Connect(string userUrl, string password, bool sideBySide)
		{
			//Calling GetClient a second time in a running process will
			//return the previously cached client. For example, calling GetClient(bool sideBySideFlag)
			// the first time in a process returns a new endpoint.  Calling the method a second
			//time returns the original endpoint. If you call GetClient(false) to get a client 
			//endpoint and then GetClient(true), the original client enpoint is returned even though
			// a true value argument is passed with the second call.

			try
			{
				_userUri = userUrl;
				_userPassword = password;

				if (_lyncClient == null)
				{
					//If sideBySide == false, a standard endpoint is created
					//Otherwise, a side-by-side endpoint is created
					_lyncClient = LyncClient.GetClient(sideBySide);
				}
				_inSideBySideMode = sideBySide;

				//Display the current state of the Lync client.
				if (ClientStateChanged != null)
				{
					ClientStateChanged(_lyncClient.State.ToString());
				}

				//Register for the three Lync client events needed so that application is notified when:
				// * Lync client signs in or out
				_lyncClient.StateChanged += OnLyncClientStateChanged;
				_lyncClient.SignInDelayed += OnLyncClientSignInDelayed;
				_lyncClient.CredentialRequested += OnLyncClientCredentialRequested;



				//Client state of uninitialized means that Lync is configured for UI suppression mode and
				//must be initialized before a user can sign in to Lync
				if (_lyncClient.State == ClientState.Uninitialized)
				{
					_lyncClient.BeginInitialize(
						(ar) =>
						{
							_lyncClient.EndInitialize(ar);
							_thisProcessInitializedLync = true;
						},
						null);
				}

				else if (_lyncClient.State == ClientState.SignedIn)
				{
					if (UserIsSignedIn != null)
					{
						UserIsSignedIn(_lyncClient);
					}

				}
				//If the Lync client is signed out, sign into the Lync client
				else if (_lyncClient.State == ClientState.SignedOut)
				{
					SignUserIn();
				}
				else if (_lyncClient.State == ClientState.SigningIn)
				{

				}
			}
			catch (NotInitializedException ni)
			{


			}
			catch (ClientNotFoundException cnf)
			{

			}
			catch (Exception exc)
			{

			}
		}

		/// <summary>
		/// Signs a user in to Lync as one of two possible users. User that is
		/// signed in depends on whether side-by-side client is chosen.
		/// </summary>
		internal void SignUserIn()
		{

			//Set the sign in credentials of the user to the
			//appropriate credentials for the endpoint mode
			string userUri = _userUri;
			string userPassword = _userPassword;


			_userUri = userUri;
			_lyncClient.BeginSignIn(
				userUri,
				userUri,
				userPassword,
				(ar) =>
				{
					try
					{
						_lyncClient.EndSignIn(ar);
					}
					catch (Exception exc)
					{
						throw exc;
					}
				},
				null);
		}


		/// <summary>
		/// Raised when user's credentials are rejected by Lync or a service that
		/// Lync depends on requests credentials
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLyncClientCredentialRequested(object sender, CredentialRequestedEventArgs e)
		{
			//If the request for credentials comes from Lync server then sign out, get new creentials
			//and sign in.
			if (e.Type == CredentialRequestedType.LyncAutodiscover)
			{
				try
				{
					_lyncClient.BeginSignOut((ar) =>
					{
						_lyncClient.EndSignOut(ar);
						//Ask user for credentials and attempt to sign in again
						SignUserIn();
					}, null);
				}
				catch (Exception ex)
				{

				}
			}
			else
			{


			}
		}

		private void OnLyncClientSignInDelayed(object sender, SignInDelayedEventArgs e)
		{
			try
			{
				_lyncClient.BeginSignOut((ar) => { _lyncClient.EndSignOut(ar); }, null);
			}
			catch (LyncClientException lce)
			{
				//	MessageBox.Show("Exception on sign out in SignInDelayed event: " + lce.Message);
			}
		}

		/// <summary>
		/// Handles the event raised when a user signs in to or out of the Lync client.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnLyncClientStateChanged(object sender, ClientStateChangedEventArgs e)
		{
			switch (e.NewState)
			{
				case ClientState.SignedOut:
					if (e.OldState == ClientState.Initializing)
					{
						SignUserIn();
					}
					if (e.OldState == ClientState.SigningOut)
					{
						_lyncClient.BeginShutdown((ar) =>
						{
							_lyncClient.EndShutdown(ar);
						}, null);
					}
					break;
				case ClientState.Uninitialized:
					if (e.OldState == ClientState.ShuttingDown)
					{
						_lyncClient.StateChanged -= OnLyncClientStateChanged;
						try
						{

						}
						catch (InvalidOperationException oe)
						{
							System.Diagnostics.Debug.WriteLine("Invalid operation exception on close: " + oe.Message);
						}
					}
					break;
				case ClientState.SignedIn:
					if (UserIsSignedIn != null)
					{
						UserIsSignedIn(_lyncClient);
					}
					break;
			}
			if (ClientStateChanged != null)
			{
				ClientStateChanged(e.NewState.ToString());
			}


		}


	}



}
