using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Parsing.MailboxLogs;
using MailboxLogParser.Common.Reporting;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace MailboxLogParser.Reporting.Reports
{
    internal class PingAnalysisReport : ReportBase
    {
        // Ping command specific parsed information
        public const string PingHeartBeatInterval = "PingHeartBeatInterval";
        public const string PingRequestFoldersText = "PingRequestFoldersText";
        public const string PingResponseFoldersText = "PingResponseFoldersText";

        public PingAnalysisReport()
            : base("PingAnalysisReport",
                new string[] {
                    PingHeartBeatInterval,
                    PingRequestFoldersText,
                    PingResponseFoldersText
                })
        {

        }

        internal override void ProcessLogEntry(MailboxLogEntry entry)
        {
            ReportRowBase row = new DummyReportRow();

            if (entry.Command != "Ping") { return; }

            row[PingAnalysisReport.PingHeartBeatInterval] = ParseHelper.ParseStringBetweenTokens(entry.RequestBody, "<HeartbeatInterval>", "</HeartbeatInterval>");

            if (entry.RequestBody.Trim().Length > 0 && entry.RequestBody.Contains("xml"))
            {
                XElement root = XElement.Parse(entry.RequestBody.Trim());
                XNamespace ns = root.Attribute("xmlns").Value;

                IEnumerable<XElement> folders =
                    from f in root.Descendants(ns + "Folder")
                    select (XElement)f;

                string foldersText = string.Empty;
                foreach (XElement folder in folders)
                {
                    if (foldersText.Length > 0)
                    {
                        foldersText = foldersText + ", ";
                    }

                    foldersText = foldersText + (string)folder.Element(ns + "Id") + " (" + (string)folder.Element(ns + "Class") + ")";
                }

                row[PingAnalysisReport.PingRequestFoldersText] = foldersText;
            }

            if (entry.ResponseBody.Trim().Length > 0 && entry.ResponseBody.Contains("xml"))
            {
                XElement root = XElement.Parse(entry.ResponseBody.Trim());
                XNamespace ns2 = root.Attribute("xmlns").Value;

                IEnumerable<XElement> folders2 =
                    from f in root.Descendants(ns2 + "Folder")
                    select (XElement)f;

                string foldersText2 = string.Empty;
                foreach (XElement folder2 in folders2)
                {
                    if (foldersText2.Length > 0)
                    {
                        foldersText2 = foldersText2 + ", ";
                    }

                    foldersText2 = foldersText2 + folder2.Value;
                }

                row[PingAnalysisReport.PingResponseFoldersText] = foldersText2;
            }

            this.ReportRows.Add(row);
        }
    }
}
