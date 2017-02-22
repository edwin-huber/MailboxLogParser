using MailboxLogParser.Common.Parsing.MailboxLogs;

namespace MailboxLogParser.Common.Reporting.BasicReport
{
    internal class BasicReportRow : ReportRowBase
    {
        protected MailboxLogEntry LogEntry { get; private set; }

        internal BasicReportRow(MailboxLogEntry logEntry) :
            base(logEntry.Name + logEntry.Identifier)
        {
            this.LogEntry = logEntry;
        }

        protected override string GetRawData()
        {
            return this.LogEntry.RawData;
        }
    }
}
