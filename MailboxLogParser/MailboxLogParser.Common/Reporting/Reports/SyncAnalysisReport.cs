using MailboxLogParser.Common.Parsing.MailboxLogs;
using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Parsing.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MailboxLogParser.Common.Reporting.Reports
{
    internal class SyncAnalysisReport : ReportBase
    {
        // Sync parsed information
        private const string SyncHeartbeatInterval = "SyncHeartbeatInterval";
        private const string SyncRequestFetchCountPerCollection = "SyncRequestFetchCountPerCollection";
        private const string SyncRequestFetchServerIdsPerCollection = "SyncRequestFetchServerIdsPerCollection";
        private const string SyncRequestCollectionIds = "SyncRequestCollectionIds";
        private const string SyncWindowSizes = "SyncWindowSizes";
        private const string SyncRequestKeys = "SyncRequestKeys";
        private const string SyncDuplicateCounts = "SyncDuplicateCounts";
        private const string SyncLastDuplicateIntervals = "SyncLastDuplicateIntervals";
        private const string SyncResponseKeys = "SyncResponseKeys";
        private const string SyncResponseAdds = "SyncResponseAdds";

        private Dictionary<string, int> CurrentCollectionIdSyncKey = new Dictionary<string, int>();

        public SyncAnalysisReport()
            : base("SyncAnalysisReport",
                new string[] {
                    SyncAnalysisReport.SyncRequestKeys,
                    SyncAnalysisReport.SyncResponseKeys,
                    SyncAnalysisReport.SyncHeartbeatInterval,
                    SyncAnalysisReport.SyncDuplicateCounts,
                    SyncAnalysisReport.SyncRequestCollectionIds,
                    SyncAnalysisReport.SyncRequestFetchCountPerCollection
                })
        {

        }

        internal override void ProcessLogEntry(MailboxLogEntry entry)
        {
            ReportRowBase row = new DummyReportRow();

            if (entry.Command != "Sync") { return; }

            // Sync request data
            SyncRequest sync = entry.RequestObject as SyncRequest;

            row[SyncAnalysisReport.SyncHeartbeatInterval] = sync.HeartbeatInterval.ToString();

            foreach (Collection c in sync.Collections)
            {
                AppendValueToColumn(row, SyncAnalysisReport.SyncRequestCollectionIds, c.CollectionId);
                AppendValueToColumn(row, SyncAnalysisReport.SyncWindowSizes, c.WindowSize.ToString());
                AppendValueToColumn(row, SyncAnalysisReport.SyncRequestKeys, c.SyncKey);

                // Track duplicate Sync requests in previous logs with matching CollectionId and SyncKey
                string duplicateTrackKey = c.CollectionId + c.SyncKey;
                if (!this.CurrentCollectionIdSyncKey.ContainsKey(duplicateTrackKey))
                {
                    this.CurrentCollectionIdSyncKey.Add(duplicateTrackKey, -1);
                }
                this.CurrentCollectionIdSyncKey[duplicateTrackKey]++;

                // If one or more duplicate is found the report it
                if (this.CurrentCollectionIdSyncKey[duplicateTrackKey] > 0)
                {
                    AppendValueToColumn(row, SyncAnalysisReport.SyncDuplicateCounts, this.CurrentCollectionIdSyncKey[duplicateTrackKey].ToString());
                }

                //if (duplicates.Count<ReportRow>() > 0)
                //{
                //    DateTime thisRequestTime = DateTime.Parse(row[FlatSyncAnalysisReport.RequestTime]);
                //    DateTime lastDupeRequestTime = DateTime.Parse(duplicates.First<ReportRow>()[FlatSyncAnalysisReport.RequestTime]);

                //    TimeSpan interval = new TimeSpan(thisRequestTime.Ticks - lastDupeRequestTime.Ticks);

                //    AppendValueToColumn(row, SyncAnalyzer.SyncLastDuplicateIntervals, interval.Seconds.ToString());
                //}

                // Process Commands issued in the Sync request
                if (c.Commands != null && c.Commands.Fetch != null && c.Commands.Fetch.ServerIds != null)
                {
                    // Capture just a count of the ServerIds in the Fetch
                    AppendValueToColumn(row, SyncAnalysisReport.SyncRequestFetchCountPerCollection, c.Commands.Fetch.ServerIds.Length.ToString());

                    // Capture the actual ServerIds for the Fetch
                    foreach (ServerId serverId in c.Commands.Fetch.ServerIds)
                    {
                        AppendValueToColumn(row, SyncAnalysisReport.SyncRequestFetchServerIdsPerCollection, serverId.ToString());
                    }
                }
            }

            // Sync response data
            IEnumerable<XElement> responseKeys = ParseHelper.GetXElements(entry.ResponseBody, "SyncKey");
            foreach (XElement responseKey in responseKeys)
            {
                AppendValueToColumn(row, SyncAnalysisReport.SyncResponseKeys, responseKey.Value);
            }

            //IEnumerable<XElement> collections = ParseHelper.GetXElements(entry.ResponseBody, "Collection");
            //foreach (XElement collection in collections)
            //{
            //    IEnumerable<XElement> adds = ParseHelper.GetXElements(entry.ResponseBody, "Add");
            //    AppendValueToColumn(row, SyncAnalyzer.SyncResponseAdds, adds.ToString());
            //}

            this.ReportRows.Add(row);
        }

        private static void AppendValueToColumn(ReportRowBase row, string columnName, string value)
        {
            if (row[columnName] == null || (String.IsNullOrEmpty(row[columnName].ToString())))
            {
                row[columnName] = value;
            }
            else
            {
                row[columnName] = row[columnName] + ", " + value;
            }
        }
    }
}
