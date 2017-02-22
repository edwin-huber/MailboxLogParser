using MailboxLogParser.Common.Parsing.MailboxLogs;
using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Parsing.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace MailboxLogParser.Common.Reporting.Reports
{
    public class MeetingTrackerReport : ReportBase
    {
        // Meeting tracking information
        internal const string OrganizerChanged = "OrganizerChanged";
        internal const string HasAttendeesChanged = "HasAttendeesChanged";
        internal const string MeetingChangeDescription = "MeetingChangeDescription";

        private Dictionary<string, MeetingTrackerItem> trackedMeetings = new Dictionary<string, MeetingTrackerItem>();

        public MeetingTrackerReport()
            : base("MeetingTrackerReport",
                new string[] {
                    MeetingTrackerReport.OrganizerChanged,
                    MeetingTrackerReport.HasAttendeesChanged
                })
        {

        }

        internal override void ProcessLogEntry(MailboxLogEntry entry)
        {
            ReportRowBase row = new DummyReportRow();

            // Find UID in request or response

            //IEnumerable<XElement> requestGoids = ParseHelper.GetXElements(entry.RequestBody, "GlobalObjId");

            TrackGoids(ParseHelper.GetXElements(entry.ResponseBody, "GlobalObjId"), entry, row);

            //IEnumerable<XElement> requestUids = ParseHelper.GetXElements(entry.RequestBody, "UID");

            TrackUids(ParseHelper.GetXElements(entry.ResponseBody, "UID"), entry, row);

            this.ReportRows.Add(row);
        }

        internal List<string> AnalyzerResults { get; private set; }

        private void TrackGoids(IEnumerable<XElement> goids, MailboxLogEntry entry, ReportRowBase row)
        {
            string uid = string.Empty;
            foreach (XElement goid in goids)
            {
                if (!TryConvertGoidToUid(goid.Value, out uid)) { continue; }
                MeetingTrackerItem meeting = GetTrackedMeeting(uid);

                ApplicationData applicationData = new ApplicationData(goid.Parent.Parent);

                // Check to see if the OrganizerBytes have changed...
                if (meeting.OrganizerBytes != applicationData.Organizer)
                {
                    if (meeting.OrganizerBytes != 0)
                    {
                        row.Columns[MeetingTrackerReport.OrganizerChanged] = "TRUE";
                    }
                    meeting.OrganizerBytes = applicationData.Organizer;
                }

                // Check to see if the meeting loses attendees
                if (!meeting.HasAttendees.HasValue)
                {
                    meeting.HasAttendees = applicationData.HasAttendees;
                }
                else if (meeting.HasAttendees.Value != applicationData.HasAttendees)
                {
                    row.Columns[MeetingTrackerReport.HasAttendeesChanged] = "TRUE";

                    meeting.HasAttendees = applicationData.HasAttendees;
                }
            }
        }

        private void TrackUids(IEnumerable<XElement> uids, MailboxLogEntry entry, ReportRowBase row)
        {
            foreach (XElement uid in uids)
            {
                MeetingTrackerItem meeting = GetTrackedMeeting(uid.Value);

                ApplicationData applicationData = new ApplicationData(uid.Parent);

                // Check to see if the OrganizerBytes have changed...
                if (meeting.OrganizerBytes != applicationData.OrganizerName)
                {
                    if (meeting.OrganizerBytes != 0)
                    {
                        row.Columns[MeetingTrackerReport.OrganizerChanged] = "TRUE";
                    }
                    meeting.OrganizerBytes = applicationData.OrganizerName;
                }

                // Check to see if the meeting loses attendees
                if (!meeting.HasAttendees.HasValue)
                {
                    meeting.HasAttendees = applicationData.HasAttendees;
                }
                else if (meeting.HasAttendees.Value != applicationData.HasAttendees)
                {
                    row.Columns[MeetingTrackerReport.HasAttendeesChanged] = "TRUE";

                    meeting.HasAttendees = applicationData.HasAttendees;
                }
            }
        }

        private MeetingTrackerItem GetTrackedMeeting(string uid)
        {
            if (!this.trackedMeetings.ContainsKey(uid))
            {
                this.trackedMeetings.Add(uid, new MeetingTrackerItem());
                this.trackedMeetings[uid].Uid = uid;
            }

            return this.trackedMeetings[uid];
        }

        private class MeetingTrackerItem
        {
            internal string Uid { get; set; }

            internal string ServerId { get; set; }
            internal int ToBytes { get; set; }
            internal int FromBytes { get; set; }
            internal int OrganizerBytes { get; set; }
            internal string Subject { get; set; }
            internal DateTime DtStamp { get; set; }
            internal DateTime StartTime { get; set; }
            internal DateTime EndTime { get; set; }
            internal bool? HasAttendees { get; set; }

            internal StringBuilder History { get; private set; }

            internal MeetingTrackerItem()
            {
                this.ServerId = string.Empty;
                this.ToBytes = 0;
                this.FromBytes = 0;
                this.OrganizerBytes = 0;
                this.Subject = string.Empty;
                this.DtStamp = DateTime.MinValue;
                this.StartTime = DateTime.MinValue;
                this.EndTime = DateTime.MinValue;
                this.History = new StringBuilder();
                this.HasAttendees = null;
            }
        }

        /// <summary>
        /// Convert a Base64 GloblalObjId to a Base16 UID value
        /// </summary>
        /// <param name="goid">Base64 string</param>
        /// <returns>Base16 string</returns>
        private bool TryConvertGoidToUid(string goid, out string uid)
        {
            uid = string.Empty;

            if (goid.Length == 0) { return false; }

            try
            {
                // Convert GlobalObjId to byte array
                byte[] bytes = Convert.FromBase64String(goid);

                // Use Convert to go from byte array to Base16 string
                StringBuilder ret = new StringBuilder();
                foreach (byte b in bytes)
                {
                    ret.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
                }

                uid = ret.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(true, "Exception in TryConvertGoidToUid: " + ex.GetType().FullName + ", " + ex.Message);
                return false;
            }
        }
    }
}
