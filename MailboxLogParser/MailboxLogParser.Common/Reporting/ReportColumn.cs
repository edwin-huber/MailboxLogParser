using System;

namespace MailboxLogParser.Common.Reporting
{
    public abstract class ReportColumnBase
    {
        internal abstract object CalculateColumnValue(object data);

        public int ColumnWidth { get; private set; }
        public string ColumnName { get; private set; }
        public Type ColumnValueType { get; private set; }

        public ReportColumnBase(string name, int width, Type valueType)
        {
            this.ColumnName = name;
            this.ColumnWidth = width;
            this.ColumnValueType = valueType;
        }
    }
}
