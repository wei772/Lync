namespace BuildABot.UC
{
    using System;
    /// <summary>
    /// Contains information about the conference. 
    /// </summary>
    /// <remarks></remarks>
    public class ConferenceInformation
    {
        /// <summary>
        /// Gets or sets the conference passcode.
        /// </summary>
        /// <value>The passcode.</value>
        /// <remarks></remarks>
        public string Passcode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the subject of the conference.
        /// </summary>
        /// <value>The subject.</value>
        /// <remarks></remarks>
        public string Subject
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets description of the conference.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks></remarks>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether phone access is enabled.
        /// </summary>
        /// <value><c>true</c> if phone access is enabled; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public bool PhoneAccessEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether passcode is optional for the conference.
        /// </summary>
        /// <value><c>true</c> if passcode is optional; otherwise, <c>false</c>.</value>
        /// <remarks></remarks>
        public bool IsPasscodeOptional
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the absolute date and time after which the conference can be deleted.
        /// </summary>
        /// <value>The expiry time.</value>
        /// <remarks>  
        /// The day and time must be between one year before, and 10 years after, 
        /// the current date and time on the server.
        /// If no value is supplied, expiry time is set to 8 hours.
        /// </remarks>
        public DateTime? ExpiryTime
        {
            get;
            set;
        }
    }
}
