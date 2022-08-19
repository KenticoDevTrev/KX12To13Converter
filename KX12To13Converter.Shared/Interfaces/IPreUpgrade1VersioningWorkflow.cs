using KX12To13Converter.Enums;
using System.Web.UI.WebControls;

namespace KX12To13Converter.Interfaces
{
    public interface IPreUpgrade1VersioningWorkflow
    {
        void DisableWorkflowAndClearHistory();
        VersionHistoryOperationType GetOperationType(string value);
        void HandleVersionHistory(int documentId, VersionHistoryOperationType operation, CurrentDocumentType version, PreviousState previousState);
        string PublishCheckInArchive(string path, int siteID, VersionHistoryOperationType archivedInEditModeOptType, VersionHistoryOperationType editModeNeverPublishedOptType, VersionHistoryOperationType editModeNeverPublishedPublishFromOptType, VersionHistoryOperationType editModePublishedWithFuturePublishFromOptType, VersionHistoryOperationType editModePreviouslyPublishedOptType, VersionHistoryOperationType nonPublishedArchivedNoHistoryOptType, Literal lstPreviousArchivedEdit, Literal lstNeverPublished, Literal lstUnpublishedWaitingPublishing, Literal lstPublishedWithFuturePublish, Literal lstPreviouslyPublished, Literal lstNoVersionHistory, bool reportOnly);
    }
}
