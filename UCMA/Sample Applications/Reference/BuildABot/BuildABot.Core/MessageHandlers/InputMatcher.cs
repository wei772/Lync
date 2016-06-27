using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BuildABot.Core.MessageHandlers
{
    /// <summary>
    /// Strategy class to determine whether the user input matches the provided regular expression pattern.
    /// </summary>
    public class InputMatcher
    {

        /// <summary>
        /// The pattern match.
        /// </summary>
        private Match match;

   
        /// <summary>
        /// Initializes a new instance of the <see cref="InputMatcher"/> class.
        /// </summary>
        /// <param name="regexPattern">The regular expression pattern to look for.</param>
        public InputMatcher(string regexPattern)
        {
            this.RegexPattern = regexPattern;
        }


        /// <summary>
        /// Gets or sets the regex patterns.
        /// </summary>
        /// <value>The regex patterns.</value>
        public string RegexPattern { get; set; }


        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified message; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(string message)
        {
            this.match = Regex.Match(message, this.RegexPattern, RegexOptions.IgnoreCase);
            return match.Success;
        }

        /// <summary>
        /// Gets the value (<see cref="System.String"/>) for the specified regex pattern group name.
        /// </summary>
        /// <value></value>
        public string this[string parameterName]
        {
            get
            {
                string result = null;
                if (this.match != null && this.match.Success)
                {
                    Group group = this.match.Groups[parameterName];
                    if (group.Success)
                    {
                        result = group.Value;
                    }
                }

                return result;
            }
        }
    }
}
