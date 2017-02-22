using MailboxLogParser.Common.Parsing.MailboxLogs;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MailboxLogParser.Common.Parsing.Objects
{
    public class SyncResponse : ParsedObjectBase
    {
        private readonly MailboxLogEntry LogEntry = null;
        public Collection[] Collections { get; private set; }

        private SyncResponse(MailboxLogEntry parent)
            : base(ParsedObjectType.SyncResponse)
        {
            this.LogEntry = parent;

            this.Collections = null;
        }

        public static SyncResponse Load(MailboxLogEntry entry)
        {
            SyncResponse cmd = new SyncResponse(entry);
            cmd.Parse();

            return cmd;
        }

        private void Parse()
        {
            IEnumerable<XElement> collections = ParseHelper.GetXElements(this.LogEntry.ResponseBody, "Collection");
            List<Collection> collectionList = new List<Collection>();
            foreach (XElement collection in collections)
            {
                collectionList.Add(new Collection(collection));
            }
            this.Collections = collectionList.ToArray();
        }
    }
}
