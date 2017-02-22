using MailboxLogParser.Common.Parsing.MailboxLogs;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MailboxLogParser.Common.Parsing.Objects
{
    public class SyncRequest : ParsedObjectBase
    {
        private readonly MailboxLogEntry LogEntry = null;

        public int HeartbeatInterval { get; private set; }
        public Collection[] Collections { get; private set; }

        private SyncRequest(MailboxLogEntry parent)
            : base(ParsedObjectType.SyncRequest)
        {
            this.LogEntry = parent;

            this.Collections = null;
        }

        public static SyncRequest Load(MailboxLogEntry entry)
        {
            SyncRequest cmd = new SyncRequest(entry);
            cmd.Parse();

            return cmd;
        }

        private void Parse()
        {
            XElement interval = null;

            if (ParseHelper.TryGetXElement(this.LogEntry.RequestBody, "HeartbeatInterval", out interval))
            {
                this.HeartbeatInterval = Convert.ToInt32(interval.Value);
            }

            IEnumerable<XElement> collections = ParseHelper.GetXElements(this.LogEntry.RequestBody, "Collection");
            List<Collection> collectionList = new List<Collection>();
            foreach (XElement collection in collections)
            {
                collectionList.Add(new Collection(collection));
            }
            this.Collections = collectionList.ToArray();
        }
    }

    public class Collection
    {
        public string SyncKey { get; private set; }
        public string CollectionId { get; private set; }
        public bool? DeletesAsMoves { get; private set; }
        public bool? GetChanges { get; private set; }
        public int? WindowSize { get; private set; }
        public Options Options { get; private set; }
        public Commands Commands { get; private set; }
        public Responses Responses { get; private set; }

        public Collection(XElement collection)
        {
            this.SyncKey = ParseHelper.GetElementValue(collection, "SyncKey");
            this.CollectionId = ParseHelper.GetElementValue(collection, "CollectionId");

            XElement deleteAsMoves = null;
            if (ParseHelper.TryGetXElement(collection, "DeletesAsMoves", out deleteAsMoves))
            {
                this.DeletesAsMoves = true;
            }

            XElement getChanges = null;
            if (ParseHelper.TryGetXElement(collection, "GetChanges", out getChanges))
            {
                this.GetChanges = true;
            }

            XElement windowSize = null;
            if (ParseHelper.TryGetXElement(collection, "WindowSize", out windowSize))
            {
                this.WindowSize = Convert.ToInt32(windowSize.Value);
            }

            XElement options = null;
            if (ParseHelper.TryGetXElement(collection, "Options", out options))
            {
                this.Options = new Options(options);
            }

            XElement commands = null;
            if (ParseHelper.TryGetXElement(collection, "Commands", out commands))
            {
                this.Commands = new Commands(commands);
            }

            XElement responses = null;
            if (ParseHelper.TryGetXElement(collection, "Responses", out responses))
            {
                this.Responses = new Responses(responses);
            }
        }
    }

    public class Options
    {
        public string Class { get; private set; }
        public int? FilterType { get; private set; }
        public BodyPreference BodyPreference { get; private set; }

        public Options(XElement options)
        {
            this.Class = ParseHelper.GetElementValue(options, "Class");

            XElement filterType = null;
            if (ParseHelper.TryGetXElement(options, "FilterType", out filterType))
            {
                this.FilterType = Convert.ToInt32(filterType.Value);
            }

            XElement bodyPreference = null;
            if (ParseHelper.TryGetXElement(options, "BodyPreference", out bodyPreference))
            {
                this.BodyPreference = new BodyPreference(bodyPreference);
            }
        }
    }

    public class Commands
    {
        public Fetch Fetch { get; private set; }

        public Commands(XElement commands)
        {
            XElement fetch = null;
            if (ParseHelper.TryGetXElement(commands, "Fetch", out fetch))
            {
                this.Fetch = new Fetch(fetch);
            }
        }
    }

    public class Responses
    {
        public Fetch Fetch { get; private set; }

        public Responses(XElement responses)
        {
            XElement fetch = null;
            if (ParseHelper.TryGetXElement(responses, "Fetch", out fetch))
            {
                this.Fetch = new Fetch(fetch);
            }
        }
    }

    public class Fetch
    {
        public ServerId[] ServerIds { get; private set; }
        public string Status { get; private set; }
        public ApplicationData ApplicationData { get; private set; }

        public Fetch(XElement fetch)
        {
            this.Status = ParseHelper.GetElementValue(fetch, "Status");

            var serverIds = new List<ServerId>();
            var ids = ParseHelper.GetXElements(fetch, "ServerId");
            foreach (XElement id in ids)
            {
                serverIds.Add(new ServerId(id));
            }
            this.ServerIds = serverIds.ToArray();

            XElement applicationData = null;
            if (ParseHelper.TryGetXElement(fetch, "ApplicationData", out applicationData))
            {
                this.ApplicationData = new ApplicationData(applicationData);
            }
        }
    }

    public class ApplicationData
    {
        public int To { get; private set; }
        public int From { get; private set; }
        public int Subject { get; private set; }
        public string MessageClass { get; private set; }

        // Appointment elements
        public DateTime DtStamp { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public int OrganizerName { get; private set; }
        public int OrganizerEmail { get; private set; }
        public bool HasAttendees { get; private set; }

        // MeetingRequest element
        public int Organizer { get; private set; }
        public string GlobalObjId { get; private set; }

        public ApplicationData(XElement applicationData)
        {
            XElement element = null;
            XAttribute attribute = null;

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "To", "bytes", out attribute))
            {
                this.To = Convert.ToInt32(attribute.Value);
            }

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "From", "bytes", out attribute))
            {
                this.From = Convert.ToInt32(attribute.Value);
            }

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "Subject", "bytes", out attribute))
            {
                this.Subject = Convert.ToInt32(attribute.Value);
            }

            this.MessageClass = ParseHelper.GetElementValue(applicationData, "MessageClass");

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "Organizer", "bytes", out attribute))
            {
                this.Organizer = Convert.ToInt32(attribute.Value);
            }

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "OrganizerName", "bytes", out attribute))
            {
                this.OrganizerName = Convert.ToInt32(attribute.Value);
            }

            if (ParseHelper.TryGetAttributeOfElement(applicationData, "OrganizerEmail", "bytes", out attribute))
            {
                this.OrganizerEmail = Convert.ToInt32(attribute.Value);
            }

            this.GlobalObjId = ParseHelper.GetElementValue(applicationData, "GlobalObjId");

            //if (ParseHelper.TryGetXElement(applicationData, "DtStamp", out element))
            //{
            //    this.DtStamp = DateTime.Parse(element.Value);
            //}

            //if (ParseHelper.TryGetXElement(applicationData, "StartTime", out element))
            //{
            //    this.StartTime = DateTime.Parse(element.Value);
            //}

            //if (ParseHelper.TryGetXElement(applicationData, "EndTime", out element))
            //{
            //    this.EndTime = DateTime.Parse(element.Value);
            //}

            this.HasAttendees = false;
            if (ParseHelper.TryGetXElement(applicationData, "Attendees", out element))
            {
                this.HasAttendees = true;
            }
        }
    }

    public class ServerId
    {
        public string CollectionId { get; private set; }
        public string ItemId { get; private set; }

        public ServerId(string collectionId, string itemId)
        {
            this.CollectionId = collectionId;
            this.ItemId = itemId;
        }

        public ServerId(XElement serverId)
        {
            string[] split = serverId.Value.Split(':');
            this.CollectionId = split[0];
            this.ItemId = split[1];
        }

        public override string ToString()
        {
            return this.CollectionId + "|" + this.ItemId;
        }
    }

    public class BodyPreference
    {
        public int? Type { get; private set; }
        public int? TruncationSize { get; private set; }

        public BodyPreference(XElement bodyPreference)
        {
            XElement type = null;
            if (ParseHelper.TryGetXElement(bodyPreference, "Type", out type))
            {
                this.Type = Convert.ToInt32(type.Value);
            }

            XElement truncationSize = null;
            if (ParseHelper.TryGetXElement(bodyPreference, "TruncationSize", out truncationSize))
            {
                this.TruncationSize = Convert.ToInt32(truncationSize.Value);
            }
        }
    }
}
