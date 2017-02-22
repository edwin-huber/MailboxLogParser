using System.Collections.Generic;

namespace MailboxLogParser.Common.Parsing.Lookups
{
    internal class StatusCodeLookup
    {
        internal static string LookUpStatusDefinition(string cmd, int statusCode)
        {
            if (GeneralStatusCodeTable.ContainsKey(statusCode))
            {
                return GeneralStatusCodeTable[statusCode];
            }

            if (cmd == "Sync")
            {
                return SyncStatusCodeTable[statusCode];
            }

            if (cmd == "Ping")
            {
                return PingStatusCodeTable[statusCode];
            }

            if (cmd == "MoveItems")
            {
                return MoveItemsStatusCodeTable[statusCode];
            }

            if (cmd == "FolderSync")
            {
                return FolderSyncStatusCodeTable[statusCode];
            }

            if (cmd == "MeetingResponse")
            {
                return MeetingResponseStatusCodeTable[statusCode];
            }

            return string.Empty;
        }

        #region Lookup Tables
        private static Dictionary<int, string> MeetingResponseStatusCodeTable = new Dictionary<int, string>()
        {
            {1, "Success."},
            {2, "Invalid meeting request."},
            {3, "An error occurred on the server mailbox."},
            {4, "An error occurred on the server."}
        };

        private static Dictionary<int, string> FolderSyncStatusCodeTable = new Dictionary<int, string>()
        {
            {1, "Success."},
            {6, "An error occurred on the server."},
            {9, "Synchronization key mismatch or invalid synchronization key."},
            {10, "Incorrectly formatted request."},
            {11, "An unknown error occurred."},
            {12, "Code unknown."}
        };

        private static Dictionary<int, string> PingStatusCodeTable = new Dictionary<int, string>()
        {
            {1, "The heartbeat interval expired before any changes occurred in the folders being monitored."},
            {2, "Changes occurred in at least one of the monitored folders. The response specifies the changed folders."},
            {3, "The Ping command request omitted required parameters."},
            {4, "Syntax error in Ping command request."},
            {5, "The specified heartbeat interval is outside the allowed range."},
            {6, "The Ping command request specified more than the allowed number of folders to monitor."},
            {7, "Folder hierarchy sync required."},
            {8, "An error occurred on the server."}
        };

        private static Dictionary<int, string> MoveItemsStatusCodeTable = new Dictionary<int, string>()
        {
            {1, "Invalid source collection ID or invalid source Item ID."},
            {2, "Invalid destination collection ID."},
            {3, "Success."},
            {4, "Source and destination collection IDs are the same."},
            {5, "One of the following failures occurred: the item cannot be moved to more than one item at a time, or the source or destination item was locked."},
            {7, "Source or destination item was locked."}
        };

        private static Dictionary<int, string> SyncStatusCodeTable = new Dictionary<int, string>()
        {
            {1, "Success."},
            {3, "Invalid synchronization key or synchronization state corrupted on server."},
            {4, "Protocol error. There was a semantic error in the synchronization request."},
            {5, "Server error. Server misconfiguration, temporary system issue, or bad item."},
            {6, "Error in client/server conversion."},
            {7, "Conflict matching the client and server object."},
            {8, "Object not found."},
            {9, "The Sync command cannot be completed. User account could be out of disk space."},
            {12, "The folder hierarchy has changed."},
            {13, "The Sync command request is not complete."},
            {14, "Invalid Wait or HeartbeatInterval value."},
            {15, "Invalid Sync command request. Too many collections are included in the Sync request."},
            {16, "Retry. Something on the server caused a retriable error."}
        };

        private static Dictionary<int, string> GeneralStatusCodeTable = new Dictionary<int, string>()
        {
            {101, "InvalidContent"},
            {102, "InvalidWBXML"},
            {103, "InvalidXML"},
            {104, "InvalidDateTime"},
            {105, "InvalidCombinationOfIDs"},
            {106, "InvalidIDs"},
            {107, "InvalidMIME"},
            {108, "DeviceIdMissingOrInvalid"},
            {109, "DeviceTypeMissingOrInvalid"},
            {110, "ServerError"},
            {111, "ServerErrorRetryLater"},
            {112, "ActiveDirectoryAccessDenied"},
            {113, "MailboxQuotaExceeded"},
            {114, "MailboxServerOffline"},
            {115, "SendQuoateExceeded"},
            {116, "MessageRecipientUnresolved"},
            {117, "MessageReplyNotAllowed"},
            {118, "MessagePreviouslySent"},
            {119, "MessageHasNoRecipient"},
            {120, "MailSubmissionFailed"},
            {121, "MessageReplyFailed"},
            {122, "AttachmentIsTooLarge"},
            {123, "UserHasNoMailbox"},
            {124, "UserCannotBeAnonymous"},
            {125, "UserPrincipalCouldNotBeFound"},
            {126, "UserDisabledForSync"},
            {127, "UserOnNewMailboxCannotSync"},
            {128, "UserOnLegacyMailboxCannotSync"},
            {129, "DeviceIsBlockedForThisUser"},
            {130, "AccessDenied"},
            {131, "AccountDisabled"},
            {132, "SyncStateNotFound"},
            {133, "SyncStateLocked"},
            {134, "SyncStateCorrupt"},
            {135, "SyncStateAlreadyExists"},
            {136, "SyncStateVersionInvalid"},
            {137, "CommandNotSupported"},
            {138, "VersionNotSupported"},
            {139, "DeviceNotFullyProvisionable"},
            {140, "RemoteWipeRequested"},
            {141, "LegacyDeviceOnStrictPolicy"},
            {142, "DeviceNotProvisioned"},
            {143, "PolicyRefresh"},
            {144, "InvalidPolicyKey"},
            {145, "ExternallyManagedDevicesNotAllowed"},
            {146, "NoRecurrenceInCalendar"},
            {147, "UnexpectedItemClass"},
            {148, "RemoteServerHasNoSSL"},
            {149, "InvalidStoredRequest"},
            {150, "ItemNotFound"},
            {151, "TooManyFolders"},
            {152, "NoFoldersFound"},
            {153, "ItemsLostAfterMove"},
            {154, "FailureInMoveOperation"},
            {155, "MoveCommandDisallowedForNonPersistentMoveAction"},
            {156, "MoveCommandInvalidDestinationFolder"},
            {160, "AvailabilityTooManyRecipients"},
            {161, "AvailabilityDLLimitReached"},
            {162, "AvailabilityTransientFailure"},
            {163, "AvailabilityFailure"},
            {164, "BodyPartPreferenceTypeNotSupported"},
            {165, "DeviceInformationRequired"},
            {166, "InvalidAccount"},
            {167, "AccountSendDisabled"},
            {168, "IRM_FeatureDisabled"},
            {169, "IRM_TransientError"},
            {170, "IRM_PermanentError"},
            {171, "IRM_InvalidTemplateID"},
            {172, "IRM_OperationNotPermitted"},
            {173, "NoPicture"},
            {174, "PictureTooLarge"},
            {175, "PictureLimitReached"},
            {176, "BodyPart_ConversationTooLarge"},
            {177, "MaximumDevicesReached"}
        };
        #endregion
    }
}
