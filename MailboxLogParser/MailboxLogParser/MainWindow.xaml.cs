using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Data;
using Microsoft.Win32;

namespace MailboxLogParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region fields
        private List<int> HiddenGridRowIndexes = new List<int>();
        private static StringBuilder ResponseDataBuilder = new StringBuilder();
        private ViewModel viewModel;
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ViewModel();
            dgMain.ItemsSource = viewModel.View;
        }
        #endregion

        #region Private Methods

        public static string ConvertToBase16(string base64)
        {
            string base16 = string.Empty;

            byte[] bytes = Convert.FromBase64String(base64);
            return GetStringFromBytes(bytes);
        }

        public static string GetStringFromBytes(byte[] bytes)
        {
            StringBuilder ret = new StringBuilder();
            foreach (byte b in bytes)
            {
                ret.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }

            return ret.ToString();
        }

        

        private void UpdateStatus(string statusMessage)
        {
            Debug.WriteLine("UpdateStatus called.");
            this.StatusLabel.Content = statusMessage;

            int visibleRows = this.dgMain.Items.Count;
            // ToDo:
            // int hiddenRows = this.LogReportGrid.Rows.Count - visibleRows;

            this.VisibleRowsLabel.Content = "Visible Rows: " + visibleRows;
            // this.HiddenRowsLabel.Content = "Hidden Rows: " + hiddenRows;
            this.UpdateLayout();
        }

        private void ClearLogs()
        {
            viewModel.Clear();

            this.dgMain.ItemsSource = null; //  LogReportGrid.Rows.Clear();

            this.LogDetailHeaders.Text = string.Empty;
            this.LogDetailRequest.Text = string.Empty;
            this.txtSearch.Text = string.Empty;
        }

        private void LoadLogsToGrid(string[] mailboxLogFiles)
        {
            Stopwatch total = new Stopwatch();
            total.Start();

            try
            {
                Stopwatch logs = new Stopwatch();
                logs.Start();

                try
                {
                    // Load the mailbox logs into the report
                    viewModel.LoadMailboxLogs(mailboxLogFiles);
                }
                finally
                {
                    logs.Stop();
                    Debug.WriteLine("LoadLogsToGrid: Report.LoadMailboxLogs took " + logs.ElapsedMilliseconds + " milliseconds.");
                }

                
            }
            finally
            {
                total.Stop();
                Debug.WriteLine("LoadReportToGrid: Overall took " + total.ElapsedMilliseconds + " milliseconds.");
            }
        }

        /// <summary>
        /// ToDo: This logic should eventually just pull the entries from the data grid / model into the separate
        /// view windows
        /// </summary>
        /// <param name="rowId"></param>
        private void LoadReportRowData(string rowId)
        {
            Stopwatch total = new Stopwatch();
            total.Start();

            try
            {
                var data = from r in viewModel.Report.ReportRows
                           where (string)r.Columns["Name"] == rowId
                           select r.RawData as string;

                if (data.Count() > 1)
                {
                    throw new ApplicationException("More than one match in ReportRows for RowId, '" + rowId + "'");
                }

                if (data.Count() == 0)
                {
                    this.LogDetailHeaders.Text = null;
                    this.LogDetailResponse.Text = null;
                    this.LogDetailRequest.Text = null;
                    return;
                }


                ResponseDataBuilder.Clear();
                ResponseDataBuilder.Append(data.First());

                if (ResponseDataBuilder.ToString().IndexOf("RequestBody") > 0)
                {
                    this.LogDetailHeaders.Text = ResponseDataBuilder.ToString().Remove(ResponseDataBuilder.ToString().IndexOf("RequestBody")); // data.First(); // need to filter out header only
                    // start at RequestHeader : or the very top, stop at RequestBody : 
                }

                if (ResponseDataBuilder.ToString().IndexOf("RequestBody") > 0)
                {

                    int intReqBod = ResponseDataBuilder.ToString().LastIndexOf("RequestBody");
                    if (intReqBod == -1)
                        intReqBod = 0;

                    int intRespBod = ResponseDataBuilder.ToString().LastIndexOf("ResponseHeader");
                    if (intRespBod == -1)
                    { intRespBod = intReqBod; };
                    this.LogDetailRequest.Text = ResponseDataBuilder.ToString().Substring(intReqBod, intRespBod - intReqBod); // need to filter out request // then filter response
                }
                else
                {
                    this.LogDetailRequest.Text = data.First();
                }


                if (ResponseDataBuilder.ToString().IndexOf("ResponseBody") > 0)
                {
                    this.LogDetailResponse.Text = ResponseDataBuilder.ToString().Substring(ResponseDataBuilder.ToString().LastIndexOf("ResponseHeader")); // from ResponseHeader : to end.
                }
            }
            finally
            {
                total.Stop();
                Debug.WriteLine("LoadReportRowData: Overall took " + total.ElapsedMilliseconds + " milliseconds.");
            }
        }

        #endregion

        #region UI Control button events etc

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ToDo: Set wait cursor?

                var dialog = new OpenFileDialog();
                dialog.Title = "Pick mailbox log files to parse...";
                dialog.CheckFileExists = true;
                dialog.Multiselect = true;

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        // ToDo: Set wait cursor?
                        UpdateStatus("Loading...");
                        LoadLogsToGrid(dialog.FileNames);
                        dgMain.ItemsSource = viewModel.View;
                    }
                    finally
                    {
                        // ToDo: Reset wait cursor?
                    }
                }

                UpdateStatus("Mailbox logs imported.");
            }
            finally
            {
                // ToDo: Reset wait cursor?
            }
        }

        private void dgMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = this.dgMain.SelectedItems[0] as DataRowView;

            LoadReportRowData(item["Id"].ToString());
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearLogs();
        }

        #endregion
    }
}
