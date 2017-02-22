using MailboxLogParser.Common.Parsing.MailboxLogs;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailboxLogParser.Common.Reporting
{
    public abstract class ReportBase
    {
        public string Name { get; private set; }
        public string SortColumnName { get; private set; }
        public List<ReportColumnBase> ReportColumns { get; private set; }
        public List<ReportRowBase> ReportRows { get; private set; }

        protected MailboxLog MailboxLog { get; private set; }

        protected ReportBase(string name, string[] columns)
        {
            this.Name = name;
        }

        protected ReportBase(string name, ReportColumnBase[] columns, string sortColumnName)
        {
            this.Name = name;
            this.SortColumnName = sortColumnName;

            this.ReportRows = new List<ReportRowBase>();

            this.ReportColumns = new List<ReportColumnBase>();
            this.ReportColumns.AddRange(columns);
        }

        public void LoadMailboxLogs(string[] fileNames)
        {
            this.MailboxLog = new MailboxLog();

            foreach (string fileName in fileNames)
            {
                this.MailboxLog.ImportFile(fileName);
            }

            this.LoadMailboxLog(this.MailboxLog);
        }

        public void LoadMailboxLog(MailboxLog mailboxLog)
        {
            this.MailboxLog = mailboxLog;

            foreach (MailboxLogEntry entry in mailboxLog.LogEntries)
            {
                this.ProcessLogEntry(entry);
            }
        }

        public void ExportReportToCSV(string outputFilePath)
        {
            // Delete the file if it exists
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            StringBuilder report = new StringBuilder();

            // Write column headers
            for (int i = 0; i < this.ReportColumns.Count; i++)
            {
                if (i == 0)
                {
                    AppendColumn(report, this.ReportColumns[i].ColumnName, true);
                }
                else
                {
                    AppendColumn(report, this.ReportColumns[i].ColumnName, false);
                }
            }
            report.AppendLine();

            // Write data
            foreach (ReportRowBase row in this.ReportRows)
            {
                for (int i = 0; i < this.ReportColumns.Count; i++)
                {
                    if (i == 0)
                    {
                        AppendColumn(report, row[this.ReportColumns[i].ColumnName], true);
                    }
                    else
                    {
                        AppendColumn(report, row[this.ReportColumns[i].ColumnName], false);
                    }
                }
                report.AppendLine();
            }

            File.AppendAllText(outputFilePath, report.ToString());
        }

        public void ExportRawLogEntries(string outputFilePath)
        {
            using (StreamWriter writer = new StreamWriter(outputFilePath))
            {
                foreach (MailboxLogEntry entry in this.MailboxLog.LogEntries)
                {
                    writer.Write(entry.RawData);
                }
            }
        }

        internal abstract void ProcessLogEntry(MailboxLogEntry entry);

        private static void AppendColumn(StringBuilder report, object value, bool firstColumn)
        {
            if (!firstColumn)
            {
                report.Append(",");
            }

            if (value != null)
            {
                report.Append("\"" + value + "\"");
            }
        }
    }
}
