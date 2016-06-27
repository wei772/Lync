/*******************************************************************************
*                                                       *
*   Copyright (C) Microsoft. All rights reserved.   *
*                                                       *
*******************************************************************************/
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Rtc.Collaboration.Samples.ContactCenter.WebClient.ViewModels
{
    /// <summary>
    /// Conversation creation requested event args.
    /// </summary>
    public class ConversationCreationRequestedEventArgs : EventArgs
    {
        #region constructor
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userName">User name to use.</param>
        /// <param name="phoneNumber">Phone number to use.</param>
        internal ConversationCreationRequestedEventArgs(string userName, string phoneNumber, string queueName, string productId)
        {
            this.UserName = userName;
            this.PhoneNumber = phoneNumber;
            this.QueueName = queueName;
            this.ProductId = productId;
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the user name of the user.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the phone number of the user.
        /// </summary>
        public string PhoneNumber { get; private set; }

        /// <summary>
        /// Gets the queue name.
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Gets the product id.
        /// </summary>
        public string ProductId { get; private set; }
        #endregion
    }

    /// <summary>
    /// Represents participant confirmation view model.
    /// </summary>
    public class ParticipantConfirmationViewModel : ViewModel
    {
        #region private variables

        /// <summary>
        /// User name.
        /// </summary>
        private string m_userName;

        /// <summary>
        /// Phone number.
        /// </summary>
        private string m_phoneNumber;

        /// <summary>
        /// Product id.
        /// </summary>
        private readonly string m_productId;

        /// <summary>
        /// Queue name.
        /// </summary>
        private readonly string m_queueName;

        /// <summary>
        /// Bool to denote if the user name is readonly.
        /// </summary>
        private bool m_isUserNameReadOnly;

        /// <summary>
        /// Bool to denote if the phone number is readonly.
        /// </summary>
        private bool m_isPhoneNumberReadOnly;
        #endregion

        #region Constructors

        /// <summary>
        /// Internal constructor to create a new participant confirmation view model.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="phoneNumber">Phone number.</param>
        /// <param name="productId">Product id.</param>
        /// <param name="queueName">Queue name.</param>
        internal ParticipantConfirmationViewModel(string userName, string phoneNumber, string productId, string queueName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                userName = "Anonymous";
            }
            else
            {
                //If we have a valid user name make the user name text box readonly.
                m_isUserNameReadOnly = true;
            }
            m_userName = userName;
            m_phoneNumber = phoneNumber;
            m_productId = productId;
            m_queueName = queueName;
        }

        #endregion

        #region protected overridden methods
        /// <summary>
        /// Intialize commands.
        /// </summary>
        protected override void OnInitializeCommands()
        {
            base.OnInitializeCommands();
            this.ConfirmCommand = new Command(this.ExecuteConfirmCommand, this.CanExecuteConfirmCommand);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public String UserName
        {
            get { return m_userName; }
            set
            {
                if (m_userName != value)
                {
                    m_userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public String PhoneNumber 
        {
            get { return m_phoneNumber; }
            set
            {
                if (m_phoneNumber != value)
                {
                    m_phoneNumber = value;
                    NotifyPropertyChanged("PhoneNumber");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether user name is readonly.
        /// </summary>
        public bool IsUserNameReadonly
        {
            get { return m_isUserNameReadOnly; }
            set
            {
                if (m_isUserNameReadOnly != value)
                {
                    m_isUserNameReadOnly = value;
                    NotifyPropertyChanged("IsUserNameReadOnly");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether phone number is readonly.
        /// </summary>
        public bool IsPhoneNumberReadonly
        {
            get { return m_isPhoneNumberReadOnly; }
            set
            {
                if (m_isPhoneNumberReadOnly != value)
                {
                    m_isPhoneNumberReadOnly = value;
                    NotifyPropertyChanged("IsPhoneNumberReadonly");
                }
            }
        }


        /// <summary>
        /// Get the confirm command to confirm participant details.
        /// </summary>
        public ICommand ConfirmCommand { get; private set; }

        #endregion

        #region private properties

        /// <summary>
        /// Gets the queue name.
        /// </summary>
        private string QueueName
        {
            get { return m_queueName; }
        }

        /// <summary>
        /// Product id.
        /// </summary>
        private string ProductId
        {
            get { return m_productId; }
        }
        #endregion

        #region command implementation

        /// <summary>
        /// Can execute confirm command.
        /// </summary>
        /// <param name="argument">Args.</param>
        /// <returns>True if confirm command can be executed. False othewise.</returns>
        public bool CanExecuteConfirmCommand(Object argument)
        {
            //If we have a valid user name then we can execute confirm command.
            return !String.IsNullOrEmpty(this.UserName);
        }

        /// <summary>
        /// Executes confirm command.
        /// </summary>
        /// <param name="argument">Args.</param>
        public void ExecuteConfirmCommand(Object argument)
        {
            EventHandler<ConversationCreationRequestedEventArgs> conversationCreationRequestedHandler = this.ConversationCreationRequested;
            if (conversationCreationRequestedHandler != null)
            {
                var eventArgs = new ConversationCreationRequestedEventArgs(this.UserName, this.PhoneNumber, this.QueueName, this.ProductId);
                conversationCreationRequestedHandler(this/*sender*/, eventArgs);
            }
        }
        #endregion

        #region events

        /// <summary>
        /// Conversation started.
        /// </summary>
        public event EventHandler<ConversationCreationRequestedEventArgs> ConversationCreationRequested;
        #endregion

    }
}