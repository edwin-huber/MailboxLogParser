using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Data;
using Microsoft.Win32;
using MailboxLogParser.Common.Reporting.BasicReport;
using System.Threading.Tasks;

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
        private bool canSearch = true;

        // Allows hidden row counts to be updated effectively
        public delegate void DataGridUpdated();

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ViewModel();
            dgMain.ItemsSource = viewModel.ListView;
            viewModel.FilterComplete += ViewModel_FilterComplete;
        }

        private void ViewModel_FilterComplete(object sender, EventArgs e)
        {
            enableControls();
        }
        #endregion

        #region Private Methods

        private void UpdateStatus(string statusMessage)
        {
            Debug.WriteLine("UpdateStatus called.");
            this.StatusLabel.Content = statusMessage;
            
            this.UpdateLayout();
        }

        private void enableControls()
        {
            canSearch = true;
            btnClear.IsEnabled = true;
            btnClearSearch.IsEnabled = true;
            btnExportMerged.IsEnabled = true;
            btnExportToCsv.IsEnabled = true;
            btnImport.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnExportMerged.IsEnabled = true;
            btnExportToCsv.IsEnabled = true;

            int visibleRows = this.dgMain.Items.Count;
            int hiddenRows = this.viewModel.Report.ReportRows.Count - visibleRows;
            this.HiddenRowsLabel.Content = "Hidden Rows: " + hiddenRows;
        }

        private void disableControls()
        {
            canSearch = false;
            btnClear.IsEnabled = false;
            btnClearSearch.IsEnabled = false;
            btnExportMerged.IsEnabled = false;
            btnExportToCsv.IsEnabled = false;
            btnImport.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnExportMerged.IsEnabled = false;
            btnExportToCsv.IsEnabled = false;
        }

        private void ClearLogs()
        {
            viewModel.Clear();

            this.dgMain.ItemsSource = null;

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
        private void LoadReportRowData(object report)
        {
            Stopwatch total = new Stopwatch();
            total.Start();

            try
            {
                this.LogDetailHeaders.Text = null;
                this.LogDetailResponse.Text = null;
                this.LogDetailRequest.Text = null;

                string data = viewModel.RetrieveData(report);
                
                if (data.Length == 0)
                {
                    return;
                }


                ResponseDataBuilder.Clear();

                ResponseDataBuilder.Append(data);

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
                    this.LogDetailRequest.Text = data;
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
                        dgMain.ItemsSource = viewModel.ListView;
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
                if(dgMain.Items.Count > 0)
                {
                    btnExportToCsv.IsEnabled = true;
                    btnExportMerged.IsEnabled = true;
                }
            }
        }

        private void dgMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMain.SelectedItems.Count == 1)
            {
                var item = this.dgMain.SelectedItems[0];

                LoadReportRowData(item);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearLogs();
            btnExportMerged.IsEnabled = false;
            btnExportToCsv.IsEnabled = false;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            doSearch(txtSearch.Text, "Filtered...");
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            doSearch("","Filter cleared...");
        }

        private void ctxRequestTextBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            doSearch(LogDetailRequest.SelectedText,"Filtered...");
        }

        private void ctxHeaderTextBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            doSearch(LogDetailHeaders.SelectedText, "Filtered...");
        }

        private void ctxResponseTextBoxSearch_Click(object sender, RoutedEventArgs e)
        {
            doSearch(LogDetailResponse.SelectedText, "Filtered...");
        }

        private void doSearch(string searchstring, string statusUpdate)
        {
            if (canSearch)
            {
                disableControls();
                txtSearch.Text = searchstring;
                Task t = Task.Factory.StartNew(() => viewModel.ExecuteSearch(txtSearch.Text), new System.Threading.CancellationToken(), TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());

                Task t2 = t.ContinueWith((antecedent) =>
                {
                    UpdateStatus(statusUpdate);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void btnExportMerged_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "txt";
            dialog.AddExtension = true;
            dialog.Filter = "Text File (*.txt) | *.txt";
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string outputFile = dialog.FileName;

            try
            {
                UpdateStatus("Saving file...");
                viewModel.Report.ExportRawLogEntries(outputFile);
            }
            catch (Exception saveException)
            {
                MessageBox.Show(saveException.ToString());
            }
            finally
            {
                UpdateStatus("Grid data saved to file...");
            }

            try
            {
                Process.Start(outputFile);
            }
            catch (Exception processStartEx)
            {
                MessageBox.Show(processStartEx.ToString());
            }
        }

        private void btnExportToCsv_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = "csv";
            dialog.AddExtension = true;
            dialog.Filter = "Comma Separated Values File (*.csv) | *.csv";
            dialog.CheckPathExists = true;

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string outputFile = dialog.FileName;

            try
            {
                UpdateStatus("Saving file...");
                viewModel.Report.ExportReportToCSV(outputFile);
            }
            catch (Exception saveException)
            {
                MessageBox.Show(saveException.ToString());
            }
            finally
            {
                UpdateStatus("Grid data saved to CSV file...");
            }

            try
            {
                Process.Start(outputFile);
            }
            catch (Exception processStartEx)
            {
                MessageBox.Show(processStartEx.ToString());
            }
        }
        #endregion

    }
}
