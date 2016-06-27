namespace BuildABot.Core.MessageHandlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

   

    /// <summary>
    /// Single state message handler that reads parameterized questions and answers from a static XML file with the following structure:
    /// <bot>
    ///     <parameterizedQA regexPattern="what is [term]">
    ///         <match>
    ///             <term>Term value capture by the regexPattern above</term>
    ///             <reply>Reply for above term. Example below.</reply>
    ///         </match>
    ///         <match>
    ///             <term>TLA</term>
    ///             <reply>Three-letter acronym</reply>
    ///         </match>
    ///      </parameterizedQA>
    /// </bot>
    /// </summary>
    public class StaticSingleStateMessageHandler : SingleStateMessageHandler
    {
        private Dictionary<string, Dictionary<string, string>> qas = new Dictionary<string, Dictionary<string, string>>();
        private string reply;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSingleStateMessageHandler"/> class.
        /// </summary>
        /// <param name="xmlFileName">Name of the XML file.</param>
        public StaticSingleStateMessageHandler(string xmlFileName)
        {
            if (!Path.IsPathRooted(xmlFileName))
            {
                string currentDirectory = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
                xmlFileName = Path.Combine(currentDirectory, xmlFileName);
            }

            XDocument xDoc = XDocument.Load(xmlFileName);

            var parameterizedQAs =
              from qas in xDoc.Root.Descendants("parameterizedQA")
              select new
              {
                  RegexPattern = qas.Attribute("regexPattern").Value.Replace("[term]",@"(?<term>(\w| |&|\-)+)"),
                  Matches =
                    from matches in qas.Descendants("match")
                    select new
                    {
                        Term = matches.Element("term").Value.ToLower(),
                        Reply = matches.Element("reply").Value
                    }
              };


            foreach (var parameterizedQA in parameterizedQAs)
            {
                this.qas[parameterizedQA.RegexPattern] = new Dictionary<string, string>();

                foreach (var match in parameterizedQA.Matches)
                {
                    this.qas[parameterizedQA.RegexPattern][match.Term] = match.Reply;
                }
            }
        }

        /// <summary>
        /// Determines whether this instance can handle the specified message.
        /// </summary>
        /// <param name="message">The message info.</param>
        /// <returns></returns>
        public override MessageHandlingResponse CanHandle(Message message)
        {
            MessageHandlingResponse response = new MessageHandlingResponse();
            foreach (string regexPattern in qas.Keys)
            {
                Match match = Regex.Match(message.Content, regexPattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups["term"] != null)
                {
                    string term = match.Groups["term"].Value.ToLower();

                    if (qas[regexPattern].ContainsKey(term))
                    {
                        reply = qas[regexPattern][term];
                        response.Confidence = this.DefaultConfidence;
                        break;
                    }
                    else if (qas[regexPattern].ContainsKey("default"))
                    {
                        reply = qas[regexPattern]["default"];
                        response.Confidence = this.DefaultConfidence;
                        break;
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public override Reply Handle(Message message)
        {
            // For performance purposes, the CanHandle method already retrieved the reply message.
            // Therefore, we just need to return it here.
            return new Reply(reply);
        }
    }
}

