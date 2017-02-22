using MailboxLogParser.Common.Parsing.Objects;
using System;
using System.Linq;
using System.Text;

namespace MailboxLogParser.Common.Parsing.MailboxLogs
{
    public class MailboxLogEntry
    {
        private readonly MailboxLog Parent = null;

        #region Log Section Properties

        public string RawData { get; private set; }

        public string Name { get; private set; }

        public DateTime? RequestTime { get; private set; }

        public string ServerName { get; private set; }

        public string AssemblyVersion { get; private set; }

        public string Identifier { get; private set; }

        public string RequestHeader { get; private set; }

        public string RequestBody { get; private set; }

        public string ExceptionInfo { get; private set; }

        public string WasPending { get; private set; }

        public string AccessState { get; private set; }

        public string AccessStateReason { get; private set; }

        public string DeviceAccessControlRule { get; private set; }

        public string ResponseHeader { get; private set; }

        public string ResponseBody { get; private set; }

        public DateTime? ResponseTime { get; private set; }

        public string RoundTripSeconds { get; private set; }

        #endregion

        #region Parsed Data

        public string Command { get; private set; }

        public string UserAgent { get; private set; }

        public string ProtocolVersion { get; private set; }

        public string HttpStatus { get; private set; }

        public string Status { get; private set; }

        public string TimeSinceLastRequestInMailboxLog { get; private set; }

        public ParsedObjectBase RequestObject { get; private set; }

        public ParsedObjectBase ResponseObject { get; private set; }

        #endregion

        private MailboxLogEntry(MailboxLog parent)
        {
            this.Parent = parent;
        }

        public static bool Initialize(MailboxLog parent, string[] lines, out MailboxLogEntry entry)
        {
            entry = new MailboxLogEntry(parent);
            entry.InitializeReally(lines);
            return true;
        }

        private void InitializeReally(string[] lines)
        {
            var builder = new StringBuilder();
            foreach (string line in lines)
            {
                builder.AppendLine(line);
            }
            this.RawData = builder.ToString();

            string tempTimeString = null;
            DateTime tempTime = DateTime.MinValue;

            this.Name = FindSection(lines, "Log Entry: ", true, false).Trim();

            this.RequestTime = null;
            tempTimeString = FindSection(lines, "RequestTime : ").Trim();
            if (DateTime.TryParse(tempTimeString, out tempTime))
            {
                this.RequestTime = tempTime;
            }

            this.ServerName = FindSection(lines, "ServerName : ").Trim();
            this.AssemblyVersion = FindSection(lines, "AssemblyVersion : ").Trim();
            this.Identifier = FindSection(lines, "Identifier : ").Trim();
            this.RequestHeader = FindSection(lines, "RequestHeader : ");
            this.RequestBody = FindSection(lines, "RequestBody : ");
            this.WasPending = FindSection(lines, "WasPending : ").Trim();
            this.AccessState = FindSection(lines, "AccessState : ").Trim();
            this.ExceptionInfo = FindSection(lines, "_Exception : ", false, true).Trim();
            this.AccessStateReason = FindSection(lines, "AccessStateReason : ").Trim();
            this.DeviceAccessControlRule = FindSection(lines, "AccessStateReason : ").Trim();
            this.ResponseHeader = FindSection(lines, "ResponseHeader : ");
            this.ResponseBody = FindSection(lines, "ResponseBody : ");

            tempTime = DateTime.MinValue;
            tempTimeString = null;

            tempTimeString = FindSection(lines, "ResponseTime : ").Trim();
            if (DateTime.TryParse(tempTimeString, out tempTime))
            {
                this.ResponseTime = tempTime;
            }

            if (this.RequestTime.HasValue && this.ResponseTime.HasValue)
            {
                TimeSpan? span = this.ResponseTime - this.RequestTime;
                if (span.HasValue)
                {
                    this.RoundTripSeconds = span.Value.TotalSeconds.ToString();
                }
            }

            this.Command = ParseHelper.ParseStringBetweenTokens(this.RequestHeader, "Cmd=", "&", " ");
            this.UserAgent = ParseHelper.ParseStringBetweenTokens(this.RequestHeader, "User-Agent: ", "\r\n");
            this.ProtocolVersion = ParseHelper.ParseStringBetweenTokens(this.RequestHeader, "MS-ASProtocolVersion: ", "\r\n");
            this.HttpStatus = ParseHelper.ParseStringBetweenTokens(this.ResponseHeader, "HTTP/1.1 ", " ");
            this.Status = ParseHelper.ParseStringBetweenTokens(this.ResponseBody, "<Status>", "</Status>");

            if (this.Parent.LogEntries.Count > 0 && this.RequestTime.HasValue && this.Parent.LogEntries.Last<MailboxLogEntry>().RequestTime.HasValue && this.Parent.LogEntries.Last<MailboxLogEntry>().RequestTime.Value != DateTime.MinValue)
            {
                TimeSpan interval = new TimeSpan(this.RequestTime.Value.Ticks - this.Parent.LogEntries.Last<MailboxLogEntry>().RequestTime.Value.Ticks);
                this.TimeSinceLastRequestInMailboxLog = interval.TotalSeconds.ToString();
            }
        }

        private string FindSection(string[] lines, string sectionHeader)
        {
            return FindSection(lines, sectionHeader, true, true);
        }

        private string FindSection(string[] lines, string sectionHeader, bool isHeaderOmitted, bool isMulitlineSection)
        {
            StringBuilder sectionBuilder = new StringBuilder();
            bool reading = false;
            string data = string.Empty;

            foreach (string line in lines)
            {
                // Return if the next section header is hit
                if (reading && line.Contains(" : "))
                {
                    return sectionBuilder.ToString();
                }

                // Start reading if the section header is found
                if (line.Contains(sectionHeader))
                {
                    reading = true;
                }

                if (isHeaderOmitted)
                {
                    data = line.Replace(sectionHeader, "");
                }
                else
                {
                    data = line;
                }

                if (reading)
                {
                    sectionBuilder.AppendLine(data);

                    // Break the loop if we don't expect to read anymore lines
                    if (!isMulitlineSection)
                    {
                        break;
                    }
                }
            }

            if (reading)
            {
                // Coming out of the loop while reading means that this was
                // the last section in the log.  Return what we've read so far.
                return sectionBuilder.ToString();
            }
            else
            {
                // Coming out of the loop without reading means the section was
                // never found.
                return string.Empty;
            }
        }
    }
}
