namespace ExcelBot.Util
{
    using System;
    using System.Xml.Linq;
    using Excel = Microsoft.Office.Interop.Excel;

    /// <summary>
    /// Helper methods for interoperating with Excel APIs.
    /// </summary>
    public class InteropHelper
    {
        /// <summary>
        /// Loads the excel spreadsheet.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        /// <param name="outputFilePath">The output file path.</param>
        /// <returns>Bot configuration information.</returns>
        internal static BotInfo LoadExcelSpreadsheet(string inputFilePath, string outputFilePath)
        {
            Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            BotInfo botInfo = new BotInfo();
            try
            {
                xlApp = new Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(inputFilePath);

                XDocument xDoc = new XDocument();
                xDoc.Add(new XElement("bot"));

                foreach (Excel.Worksheet worksheet in xlWorkBook.Worksheets)
                {
                    Excel.Range range = worksheet.UsedRange;
                    if (worksheet.Name == "botInfo")
                    {
                        for (int rowCount = 1; rowCount <= range.Rows.Count; rowCount++)
                        {
                            string propertyName =(string)(range.Cells[rowCount, 1] as Excel.Range).Value2;
                            string propertyValue =(string)(range.Cells[rowCount, 2] as Excel.Range).Value2;
                            switch (propertyName)
                            {
                                case "ApplicationUrn":
                                    botInfo.ApplicationUrn = propertyValue;
                                    break;
                                case "ApplicationUserAgent":
                                    botInfo.ApplicationUserAgent = propertyValue;
                                    break;                                                            
                            }
                        }
                    }
                    else
                    {
                        XElement qaRootNode;
                        if (worksheet.Name == "static")
                        {
                            qaRootNode = new XElement("parameterlessQAs");
                            PopulateQARootNode(range, qaRootNode, "qa", "question", "answer");

                        }
                        else
                        {
                            qaRootNode = new XElement("parameterizedQA");
                            qaRootNode.Add(new XAttribute("regexPattern", worksheet.Name.Replace('<', '[').Replace('>', ']')));
                            PopulateQARootNode(range, qaRootNode, "match", "term", "reply");

                        }
                        xDoc.Root.Add(qaRootNode);
                        ReleaseObject(worksheet);
                    }
                }

                xDoc.Save(outputFilePath);
                return botInfo;

            }
            finally
            {
                if (xlWorkBook != null)
                {
                    xlWorkBook.Close(true, null, null);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                }

                ReleaseObject(xlWorkBook);
                ReleaseObject(xlApp);
            }

        }

        /// <summary>
        /// Populates the QA root node.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="qaRootNode">The qa root node.</param>
        /// <param name="rootName">Name of the root.</param>
        /// <param name="firstChildName">First name of the child.</param>
        /// <param name="secondChildName">Name of the second child.</param>
        private static void PopulateQARootNode(Excel.Range range, XElement qaRootNode, string rootName, string firstChildName, string secondChildName)
        {
            for (int rowCount = 1; rowCount <= range.Rows.Count; rowCount++)
            {
                string firstChildValue = (string)(range.Cells[rowCount, 1] as Excel.Range).Value2;
                string secondChildValue = (string)(range.Cells[rowCount, 2] as Excel.Range).Value2;
                XElement qa = new XElement(rootName,
                    new XElement(firstChildName, firstChildValue),
                    new XElement(secondChildName, secondChildValue));
                qaRootNode.Add(qa);
            }
        }

        /// <summary>
        /// Releases the object.
        /// </summary>
        /// <param name="obj">The object.</param>
        private static void ReleaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch
            {
                obj = null;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}
