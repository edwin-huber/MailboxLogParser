using System.Collections.Generic;

namespace MailboxLogParser.Common.Reporting
{
    public abstract class ReportRowBase
    {
        public string RowId { get; private set; }
        public string RawData
        {
            get
            {
                return GetRawData();
            }
        }

        public Dictionary<string, object> Columns { get; private set; }

        public ReportRowBase(string rowId)
        {
            this.Columns = new Dictionary<string, object>();
            this.RowId = rowId;
        }

        public object this[string key]
        {
            get
            {
                if (this.Columns.ContainsKey(key))
                {
                    return this.Columns[key];
                }

                return string.Empty;
            }

            set
            {
                if (!this.Columns.ContainsKey(key))
                {
                    this.Columns.Add(key, value);
                }

                this.Columns[key] = value;
            }
        }

        protected abstract string GetRawData();
    }

    #region DummyReportRow
    public class DummyReportRow : ReportRowBase
    {
        public DummyReportRow()
            : base(System.DateTime.Now.Ticks.ToString())
        {
        }

        protected override string GetRawData()
        {
            return "This is dummy data.";
        }
    }
    #endregion
}
