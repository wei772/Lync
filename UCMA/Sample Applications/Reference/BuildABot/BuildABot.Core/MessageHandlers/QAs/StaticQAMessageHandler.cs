namespace BuildABot.Core.MessageHandlers.QAs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Handles static questions and answers defined in a XML file with the following structure:
    /// <bot>
    ///     <parameterlessQAs>
    ///         <qa>
    ///             <question>question1</question>
    ///             <answer>answer1</answer>
    ///         </qa>
    ///         <qa>
    ///             <question>question2</question>
    ///             <answer>answer2a</answer>
    ///             <answer>answer2b</answer>
    ///         </qa>
    ///         ...
    ///     </parameterlessQAs>
    /// </bot>
    /// </summary>
    public class StaticQAMessageHandler : QAMessageHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticQAMessageHandler"/> class.
        /// </summary>
        /// <param name="xmlFileName">Name of the XML file.</param>
        public StaticQAMessageHandler(string xmlFileName)
            : base()
        {
            XDocument xDoc = XDocument.Load(xmlFileName);

            var randomQAs =
              from n in xDoc.Root.Descendants("parameterlessQAs").Descendants("qa")
              select new RandomQA(
                n.Element("question").Value,
                (from answer in n.Elements("answer") select answer.Value).ToArray());

            foreach (RandomQA randomQA in randomQAs)
            {
                this.QAs.Add(randomQA);
            }
        }
    }
}
