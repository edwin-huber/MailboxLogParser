using MailboxLogParser.Common.Parsing;
using MailboxLogParser.Common.Parsing.Lookups;
using MailboxLogParser.Common.Parsing.MailboxLogs;
using MailboxLogParser.Common.Parsing.Objects;
using System;
using System.Linq;

namespace MailboxLogParser.Common.Reporting.BasicReport
{
    #region BasicReportColumnBase

    internal abstract class BasicReportColumnBase : ReportColumnBase
    {
        internal BasicReportColumnBase(string name, int width, Type valueType)
            : base(name, width, valueType)
        {

        }

        protected static MailboxLogEntry ConvertToMailboxLogEntry(object data)
        {
            var log = data as MailboxLogEntry;

            if (log == null)
            {
                throw new ArgumentException("Argument is null or not of type MailboxLogParser.Common.Parsing.MailboxLogs.MailboxLogEntry", "data");
            }

            return log;
        }
    }

    #endregion

    #region NameColumn

    internal class NameColumn : BasicReportColumnBase
    {
        internal NameColumn()
            : base("Name", 30, typeof(int))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.Name;
        }
    }

    #endregion

    #region RequestTimeColumn
    internal class RequestTimeColumn : BasicReportColumnBase
    {
        internal RequestTimeColumn()
            : base("RequestTime", 125, typeof(DateTime))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.RequestTime;
        }
    }
    #endregion

    #region ServerNameColumn

    internal class ServerNameColumn : BasicReportColumnBase
    {
        internal ServerNameColumn()
            : base("ServerName", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.ServerName;
        }
    }
    #endregion

    #region AssemblyVersionColumn
    internal class AssemblyVersionColumn : BasicReportColumnBase
    {
        internal AssemblyVersionColumn()
            : base("AssemblyVersion", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.AssemblyVersion;
        }
    }

    #endregion

    #region MailboxNameColumn
    internal class MailboxNameColumn : BasicReportColumnBase
    {
        internal MailboxNameColumn()
            : base("MailboxName", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.RequestHeader, "User=", "&");
        }
    }

    #endregion

    #region IdentifierColumn
    internal class IdentifierColumn : BasicReportColumnBase
    {
        internal IdentifierColumn()
            : base("Identifier", 80, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.Identifier;
        }
    }

    #endregion

    #region WasPendingColumn

    internal class WasPendingColumn : BasicReportColumnBase
    {
        internal WasPendingColumn()
            : base("WasPending", 50, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.WasPending;
        }
    }

    #endregion

    #region AccessStateColumn

    internal class AccessStateColumn : BasicReportColumnBase
    {
        internal AccessStateColumn()
            : base("AccessState", 75, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.AccessState;
        }
    }

    #endregion

    #region AccessStateReasonColumn

    internal class AccessStateReasonColumn : BasicReportColumnBase
    {
        internal AccessStateReasonColumn()
            : base("AccessStateReason", 75, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.AccessStateReason;
        }
    }

    #endregion

    #region DeviceAccessControlRuleColumn

    internal class DeviceAccessControlRuleColumn : BasicReportColumnBase
    {
        internal DeviceAccessControlRuleColumn()
            : base("DeviceAccessControlRule", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.DeviceAccessControlRule;
        }
    }

    #endregion

    #region ResponseTimeColumn

    internal class ResponseTimeColumn : BasicReportColumnBase
    {
        internal ResponseTimeColumn()
            : base("ResponseTime", 125, typeof(DateTime))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.ResponseTime;
        }
    }

    #endregion

    #region CommandColumn

    internal class CommandColumn : BasicReportColumnBase
    {
        internal CommandColumn()
            : base("Command", 75, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.RequestHeader, "Cmd=", "&", " ");
        }
    }

    #endregion

    #region UserAgentColumn

    internal class UserAgentColumn : BasicReportColumnBase
    {
        internal UserAgentColumn()
            : base("UserAgent", 150, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.RequestHeader, "User-Agent: ", "\r\n");
        }
    }

    #endregion

    #region ProtocolVersionColumn

    internal class ProtocolVersionColumn : BasicReportColumnBase
    {
        internal ProtocolVersionColumn()
            : base("ProtocolVersion", 80, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.RequestHeader, "MS-ASProtocolVersion: ", "\r\n");
        }
    }

    #endregion

    #region HttpStatusColumn

    internal class HttpStatusColumn : BasicReportColumnBase
    {
        internal HttpStatusColumn()
            : base("HttpStatus", 75, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.ResponseHeader, "HTTP/1.1 ", " ");
        }
    }

    #endregion

    #region StatusColumn

    internal class StatusColumn : BasicReportColumnBase
    {
        internal StatusColumn()
            : base("Status", 75, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return ParseHelper.ParseStringBetweenTokens(entry.ResponseBody, "<Status>", "</Status>");
        }
    }

    #endregion

    #region StatusDefinitionColumn

    internal class StatusDefinitionColumn : BasicReportColumnBase
    {
        internal StatusDefinitionColumn()
            : base("StatusDefinition", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            if (entry.Status.Length > 0)
            {
                return StatusCodeLookup.LookUpStatusDefinition(entry.Command, Convert.ToInt32(entry.Status));
            }

            return null;
        }
    }

    #endregion

    #region ExceptionInfoColumn

    internal class ExceptionInfoColumn : BasicReportColumnBase
    {
        internal ExceptionInfoColumn()
            : base("ExceptionInfo", 100, typeof(string))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            if (entry.ExceptionInfo != null && entry.ExceptionInfo.Length > 0)
            {
                return entry.ExceptionInfo.Substring(0, entry.ExceptionInfo.IndexOf("\n")).Replace(" :", "");
            }

            return null;
        }
    }

    #endregion

    #region ContainsSyncKeyZeroColumn

    internal class ContainsSyncKeyZeroColumn : BasicReportColumnBase
    {
        internal ContainsSyncKeyZeroColumn()
            : base("ContainsSyncKeyZero", 100, typeof(bool))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            if (entry.Command == "Sync")
            {
                var request = SyncRequest.Load(entry);
                var collections = from c in request.Collections
                                  where c.SyncKey == "0"
                                  select c as Collection;

                if (collections.Count() > 0)
                {
                    return true;
                }
            }

            return null;
        }
    }

    #endregion

    #region RoundTripSecondsColumn

    internal class RoundTripSecondsColumn : BasicReportColumnBase
    {
        internal RoundTripSecondsColumn()
            : base("RoundTripSeconds", 100, typeof(int))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            if (entry.RequestTime.HasValue && entry.ResponseTime.HasValue)
            {
                TimeSpan? span = entry.ResponseTime - entry.RequestTime;

                if (span.HasValue)
                {
                    return span.Value.TotalSeconds;
                }
            }

            return null;
        }
    }

    #endregion

    #region RequestBodyLengthColumn

    internal class RequestBodyLengthColumn : BasicReportColumnBase
    {
        internal RequestBodyLengthColumn()
            : base("RequestBodyLength", 100, typeof(int))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.RequestBody.Length;
        }
    }

    #endregion

    #region ResponseBodyLengthColumn

    internal class ResponseBodyLengthColumn : BasicReportColumnBase
    {
        internal ResponseBodyLengthColumn()
            : base("ResponseBodyLength", 100, typeof(int))
        {

        }

        internal override object CalculateColumnValue(object data)
        {
            var entry = ConvertToMailboxLogEntry(data);

            return entry.ResponseBody.Length;
        }
    }

    #endregion
}
