namespace BuildABot.UC
{
    using System.Text.RegularExpressions;
    /// <summary>
    /// Uc Bot Host Helper.
    /// </summary>
    public static class UcBotHostHelper
    {
        /// <summary>
        /// Gets or sets the current host.
        /// </summary>
        /// <value>
        /// The current host.
        /// </value>
        public static UCBotHost CurrentHost { get; set; }

        /// <summary>
        /// Tries to validate provided conference URI and return updated Uri and will state if it is valid or not.
        ///  1. Correct conference URI should start with SIP. 
        ///    If developer created a conference, it will add "conf:". Method will strip it.
        /// 2. Also need to trim any conversation-id if it was specified.
        ///    If developer specified the conversation id it will cause invalid uri SipException so method will strip it as well.
        ///    
        /// <example>
        /// Valid Conference Uri Example:
        /// 
        /// sip:useralias@microsoft.com;gruu;opaque=app:conf:focus:id:E339C29D99BBE4429930B21B0B623175
        /// </example>
        /// </summary>
        /// <param name="conferenceUri">The conference URI.</param>
        /// <returns>True if conference uri is valid, otherwise false.</returns>
        public static bool TryValidateConferenceUri(ref string conferenceUri)
        {
            if (!string.IsNullOrEmpty(conferenceUri))
            {
                // While there is an example of what need to be specified, we still need to handle these situations
                // 1. Correct conference URI should start with SIP. 
                //    If developer created a conference, it will add "conf:" at the beginning, and if developer didn't read the example, we need to help out with building correct uri.
                // 2. Also need to trim any conversation-id if it was specified.
                //    If developer specified the conversation id it will cause invalid uri SipException.
                conferenceUri = Regex.Replace(conferenceUri, "(^conf:[_]?){1}|(%3Fconversation-id=[A-z0-9=+-_]{32}){1}", string.Empty);

                return Regex.IsMatch(conferenceUri, "sip:[A-z0-9_.]+\\@microsoft.com;gruu;opaque=app:conf:focus:id:[A-z0-9=+_-]{32}");

            }
            return false;
        }
    }
}
