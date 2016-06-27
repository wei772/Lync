namespace BuildABot.Util
{
    using System.Xml;

    /// <summary>
    /// Helper methods for manipulating XML nodes.
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Gets the inner XML of a node or returns empty if the node is null.
        /// </summary>
        /// <param name="node">The node.</param>
        public static string GetInnerXmlOrEmpty(this XmlNode node)
        {
            string result = string.Empty;
            if (node != null)
            {
                result = node.InnerText;
            }
            return result;
        }
    }
}
