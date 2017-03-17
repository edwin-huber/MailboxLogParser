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
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows;

namespace MailboxLogParser
{
    class ViewModel
    {
        public ReportBase Report = new BasicReport();

        public ObservableCollection<ReportRowBase> ListView;
        private CollectionView view;
        private IEnumerable<ReportRowBase> searchHitRows;

        public event EventHandler FilterComplete;

        protected virtual void OnFilterComplete(EventArgs e)
        {
            FilterComplete?.Invoke(this, e);
        }

        /// <summary>
        /// Not thread safe, as we are updating the DataTable
        /// </summary>
        /// <param name="mailboxLogFiles"></param>
        public void LoadMailboxLogs(string[] mailboxLogFiles)
        {
            this.Report.LoadMailboxLogs(mailboxLogFiles);

            view = (CollectionView) CollectionViewSource.GetDefaultView(Report.ReportRows);
            ListView = new ObservableCollection<ReportRowBase>();
            updateListView();
        }

        private void updateListView()
        {
            Application.Current.Dispatcher.Invoke(() =>
           {
               ListView.Clear();
               foreach (var row in view)
               {
                   ListView.Add((ReportRowBase)row);
               }
           });
        }

        internal void Clear()
        {
            this.Report = new BasicReport();
            ListView = null;
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
                                select r
                                    ;

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
                Task<CollectionView> t1 = Task.Factory.StartNew((arg) =>
                {
                    ReportBase report = (ReportBase)arg;
                    CollectionView internalview = (CollectionView)CollectionViewSource.GetDefaultView(searchHitRows);


                    // internalview.Filter = new Predicate<object>(RowInFilter);
                    return internalview;
                }, Report, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default);

                Task t2 = t1.ContinueWith((antecedent) =>
                {
                    view = antecedent.Result;
                }, new CancellationToken(), TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

                Task t3 = t2.ContinueWith((antecedent) =>
                {
                    updateListView();
                    OnFilterComplete(EventArgs.Empty);
                    loop.Stop();
                    Debug.WriteLine("ExecuteSearch: DataGrid predicate filter on ListView took " + loop.ElapsedMilliseconds + " milliseconds.");

                });
            }
            catch (AggregateException ae)
            {
                 Debug.WriteLine(ae.ToString());
            }
        }

        internal string RetrieveData(object report)
        {
            ReportRowBase rep = report as ReportRowBase;

            return rep.RawData;
        }

    }
}
