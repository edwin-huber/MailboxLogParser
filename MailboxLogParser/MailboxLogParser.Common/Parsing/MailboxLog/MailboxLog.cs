using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MailboxLogParser.Common.Parsing.MailboxLogs
{
    public class MailboxLog
    {
        public readonly Collection<MailboxLogEntry> LogEntries = new Collection<MailboxLogEntry>();

        public void ImportFile(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new ArgumentException("Path does not exist", inputPath);
            }

            List<string> data = null;
            string nextLine = string.Empty;
            bool result = true;
            MailboxLogEntry entry = null;

            using (StreamReader reader = new StreamReader(inputPath))
            {
                while (!reader.EndOfStream)
                {
                    result = TryReadNextLogEntry(reader, nextLine, out data, out nextLine);

                    if (!result && this.LogEntries.Count == 0)
                    {
                        return;
                    }

                    if (MailboxLogEntry.Initialize(this, data.ToArray(), out entry))
                    {
                        if (!IsLogEntryInCollection(entry))
                        {
                            this.LogEntries.Add(entry);
                        }
                    }
                }
            }
        }

        public void WriteLogEntries(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (MailboxLogEntry entry in this.LogEntries)
                {
                    writer.Write(entry.RawData);
                }
            }
        }

        private bool IsLogEntryInCollection(MailboxLogEntry newEntry)
        {
            var match = from entry in this.LogEntries where entry.Identifier == newEntry.Identifier select entry;

            return match.Count<MailboxLogEntry>() > 0;
        }

        private static bool IsFileValidMailboxLog(Stream inputStream)
        {
            using (StreamReader reader = new StreamReader(inputStream))
            {
                string line = null;

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    if (line.Contains("-----------------"))
                    {
                        return true;
                    }

                    if (!String.IsNullOrEmpty(line.Trim()))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool TryReadNextLogEntry(StreamReader reader, string previousLine, out List<string> lines, out string nextLine)
        {
            int logEntryNameBorders = 1;
            bool logEntryNameFound = false;
            string line = String.Empty;

            lines = new List<string>();
            nextLine = String.Empty;

            try
            {
                while (!reader.EndOfStream)
                {
                    // If a previous line was passed in, process that before reading the stream
                    if (!String.IsNullOrEmpty(previousLine) && lines.Count == 0)
                    {
                        line = previousLine;
                    }
                    else
                    {
                        line = reader.ReadLine();
                    }

                    if (line.Trim().Equals("-----------------") && logEntryNameFound)
                    {
                        logEntryNameBorders++;
                    }

                    if (line.Contains("Log Entry:"))
                    {
                        logEntryNameFound = true;
                    }

                    // If a third log entry name border was found, return it and break the loop
                    if (logEntryNameBorders == 3)
                    {
                        nextLine = line;
                        break;
                    }

                    lines.Add(line);
                }

                if (logEntryNameBorders == 0 && reader.EndOfStream)
                {
                    lines = null;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(true, "Exception in TryReadNextLogEntry: " + ex.GetType().FullName + ", " + ex.Message);
                lines = new List<string>();
                return false;
            }
        }

    }
}
