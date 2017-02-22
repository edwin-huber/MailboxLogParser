using MailboxLogParser.Common.Reporting;
using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Reporting.BasicReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace MailboxLogParser
{
    class ViewModel
    {
        public ReportBase Report = new BasicReport();
        private DataTable table;
        public DataView View;

        /// <summary>
        /// Not thread safe, as we are updating the DataTable
        /// </summary>
        /// <param name="mailboxLogFiles"></param>
        public void LoadMailboxLogs(string[] mailboxLogFiles)
        {
            this.Report.LoadMailboxLogs(mailboxLogFiles);
            int index = 0;
            table = new DataTable();
            setColumns(Report.ReportColumns);
            
            foreach (ReportRowBase reportRow in this.Report.ReportRows)
            {
                DataRow dr = table.NewRow();
                dr["Id"] = index;
                index++;

                setRow(dr, reportRow);

                table.Rows.Add(dr);
            }

            View = table.DefaultView;
        }

        private void setRow(DataRow dr, ReportRowBase reportRow)
        {
            foreach (string columnName in reportRow.Columns.Keys)
            {
                dr[columnName] = (reportRow[columnName] == null) ? DBNull.Value : reportRow[columnName] ;
            }
        }

        private void setColumns(List<ReportColumnBase> cols)
        {
            DataColumn dc;
            dc = new DataColumn("Id", Type.GetType("System.Int32")); // Col 0
            table.Columns.Add(dc);
            foreach (ReportColumnBase col in cols)
            {
                dc = new DataColumn(col.ColumnName, col.ColumnValueType);
                table.Columns.Add(dc);
            }
        }

        internal void Clear()
        {
            this.Report = new BasicReport();
            View = null;
            table = null;
        }
    }
}
