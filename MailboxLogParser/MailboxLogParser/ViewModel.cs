using MailboxLogParser.Common.Reporting;
using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Reporting.BasicReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics;
using System.Windows.Data;

namespace MailboxLogParser
{
    class ViewModel
    {
        public ReportBase Report = new BasicReport();
        private DataTable table;
        // public DataView View;
        public CollectionView ListView;
        private IEnumerable<string> searchHitRows;
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

            ListView = (CollectionView) CollectionViewSource.GetDefaultView(Report.ReportRows);
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
            ListView = null;
            table = null;
            
        }

        private static string ConvertToBase16(string base64)
        {
            string base16 = string.Empty;

            byte[] bytes = Convert.FromBase64String(base64);
            return GetStringFromBytes(bytes);
        }

        private static string GetStringFromBytes(byte[] bytes)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte b in bytes)
            {
                ret.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }

            return ret.ToString();
        }


        public void ExecuteSearch(string searchString)
        {
            Stopwatch total = new Stopwatch();
            total.Start();

            try
            {
                Stopwatch linq = new Stopwatch();
                linq.Start();

                string altText = searchString;

                if (searchString.StartsWith("[GOID]"))
                {
                    searchString = searchString.Replace("[GOID]", "");
                    altText = ConvertToBase16(searchString).Substring(40);
                }
                // Query the raw data for each row in the report to see if
                // they contain the search text
                searchHitRows = from r in Report.ReportRows
                                    where r.RawData.ToUpper().Contains(searchString.ToUpper()) || r.RawData.ToUpper().Contains(altText.ToUpper())
                                    select r.RowId as string;

                // no results bail
                if (searchHitRows.Count() == 0)
                {
                    // ToDo: not hiding all rows for now, no op is better anyway... ?
                    return;
                }

                linq.Stop();
                Debug.WriteLine("ExecuteSearch: LINQ query took " + linq.ElapsedMilliseconds + " milliseconds.");

                Stopwatch loop = new Stopwatch();
                loop.Start();

                // view should only contain rows which match the search of the report

                ListView.Filter = new Predicate<object>(RowInFilter);
                                
                loop.Stop();
                Debug.WriteLine("ExecuteSearch: DataGrid predicate filter on ListView took " + loop.ElapsedMilliseconds + " milliseconds.");
            }
            finally
            {
                total.Stop();
                Debug.WriteLine("ExecuteSearch: Overall took " + total.ElapsedMilliseconds + " milliseconds.");
            }
        }

        internal string RetrieveData(object report)
        {
            ReportRowBase rep = report as ReportRowBase;

            return rep.RawData;
        }



        /// <summary>
        /// Returns true if the row is in the filter and should be shown
        /// If we use the collection view filtering logic, but our collection
        /// in the reportrows is a little messy for this purpose, currently 
        /// using a string filter on the DefaultView on the table...
        /// Needs perf testing if this is really an issue or not
        /// </summary>
        /// <param name="reportRow"></param>
        /// <returns></returns>
        public bool RowInFilter(object reportRow)
        {
            ReportRowBase r = reportRow as ReportRowBase;

            if(searchHitRows.Contains(r.RowId as string))
            {
                return true;
            }

            return false;
        }

    }
}
