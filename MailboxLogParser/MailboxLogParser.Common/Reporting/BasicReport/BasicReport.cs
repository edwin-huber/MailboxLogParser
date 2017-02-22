using MailboxLogParser.Common.Parsing.MailboxLogs;
using System;
using System.Diagnostics;

namespace MailboxLogParser.Common.Reporting.BasicReport
{
    public class BasicReport : ReportBase
    {
        public BasicReport()
             : base("BasicReport",
                 new ReportColumnBase[] {
                    new NameColumn(),
                    new IdentifierColumn(),

                    new ServerNameColumn(),
                    new MailboxNameColumn(),
                    new UserAgentColumn(),

                    new CommandColumn(),
                    new RequestTimeColumn(),
                    new ResponseTimeColumn(),


                    new HttpStatusColumn(),
                    new StatusColumn(),
                    new StatusDefinitionColumn(),
                    new ExceptionInfoColumn(),

                    new RoundTripSecondsColumn(),
                    new RequestBodyLengthColumn(),
                    new ResponseBodyLengthColumn(),

                    new AssemblyVersionColumn(),
                    new ProtocolVersionColumn(),

                    new WasPendingColumn(),
                    new AccessStateColumn(),
                    new AccessStateReasonColumn(),
                    new DeviceAccessControlRuleColumn()
                 },
             "RequestTime")
        {

        }

        internal override void ProcessLogEntry(MailboxLogEntry logEntry)
        {
            ReportRowBase row = null;

            if (TryGetRowFromEntry(logEntry, out row))
            {
                this.ReportRows.Add(row);
            }
        }

        private bool TryGetRowFromEntry(MailboxLogEntry logEntry, out ReportRowBase newRow)
        {
            newRow = null;

            try
            {
                var row = new BasicReportRow(logEntry);

                foreach (ReportColumnBase column in this.ReportColumns)
                {
                    row[column.ColumnName] = column.CalculateColumnValue(logEntry);
                }

                newRow = row;
                return true;
            }
            catch (Exception ex)
            {
                Debug.Assert(true, "Exception in TryGetRowFromEntry: " + ex.GetType().FullName + ", " + ex.Message);
                return false;
            }
        }
    }
}
