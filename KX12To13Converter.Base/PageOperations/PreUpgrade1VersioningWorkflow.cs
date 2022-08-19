using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.WorkflowEngine;
using KX12To13Converter.Enums;
using KX12To13Converter.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using TreeNode = CMS.DocumentEngine.TreeNode;
namespace KX12To13Converter.Base.PageOperations
{
    public class PreUpgrade1VersioningWorkflow : IPreUpgrade1VersioningWorkflow
    {
        private TreeProvider _TreeProvider;
        private VersionManager _Manager;
        private WorkflowManager _WFManager;
        private int _Found;
        private int _Published;
        private int _RolledBack;
        private int _Archived;

        public PreUpgrade1VersioningWorkflow()
        {
            _TreeProvider = new TreeProvider(MembershipContext.AuthenticatedUser);
            _Manager = VersionManager.GetInstance(_TreeProvider);
            _WFManager = WorkflowManager.GetInstance(_TreeProvider);
            _Found = 0;
            _Published = 0;
            _RolledBack = 0;
            _Archived = 0;
        }

        public string PublishCheckInArchive(string path, int siteID, VersionHistoryOperationType archivedInEditModeOptType, VersionHistoryOperationType editModeNeverPublishedOptType, VersionHistoryOperationType editModeNeverPublishedPublishFromOptType, VersionHistoryOperationType editModePublishedWithFuturePublishFromOptType, VersionHistoryOperationType editModePreviouslyPublishedOptType, VersionHistoryOperationType nonPublishedArchivedNoHistoryOptType, Literal lstPreviousArchivedEdit, Literal lstNeverPublished, Literal lstUnpublishedWaitingPublishing, Literal lstPublishedWithFuturePublish, Literal lstPreviouslyPublished, Literal lstNoVersionHistory, bool reportOnly)
        {

            using (CMSActionContext context = new CMSActionContext()
            {
                LogEvents = false,
                LogSynchronization = false,
                LogExport = false,
                LogIntegration = false
            })
            {

                string siteWhere = string.Empty;
                if (siteID > 0)
                {
                    siteWhere = $" and NodeSiteID = {siteID}";
                }

                // Get Previously Archived pages that are in edit mode.
                var archivedInEditModeDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is not null and DocumentIsArchived = 0 and (Select top 1 StepType from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
 where VH.DocumentID = T.DocumentID and WasPublishedFrom is not null order by VersionHistoryID desc) = 101", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);


                var editModeNeverPublishedDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is not null and DocumentPublishedVersionHistoryID is null and DocumentIsArchived = 0
and not exists (Select * from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
where VH.DocumentID = T.DocumentID and WasPublishedTo is not null)
and not exists (Select * from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
where VH.DocumentID = T.DocumentID and ToBePublished = 1 and PublishFrom is not null)", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);

                var editModeNeverPublishedPublishFromDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is not null and DocumentPublishedVersionHistoryID is null and DocumentIsArchived = 0 and DocumentCanBePublished = 0
 and exists (Select * from CMS_VersionHistory VH
 left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
 where VH.DocumentID = T.DocumentID and ToBePublished = 1 and PublishFrom is not null)", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);

                var editModePublishedWithFuturePublishFromDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is not null and DocumentPublishedVersionHistoryID is not null and DocumentIsArchived = 0 and DocumentCanBePublished = 1
and exists (Select * from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
where VH.DocumentID = T.DocumentID and ToBePublished = 1 and PublishFrom is not null)", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);

                var editModePreviouslyPublishedDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is not null and DocumentPublishedVersionHistoryID is not null and (DocumentCheckedOutVersionHistoryID <> DocumentPublishedVersionHistoryID) and NodeAliasPath <> '/' and DocumentIsArchived = 0 and DocumentCanBePublished = 1
and (Select top 1 StepType from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
where VH.DocumentID = T.DocumentID and WasPublishedFrom is not null
order by VersionHistoryID) = 100
and (
Select top 1 StepType from CMS_VersionHistory VH
left join CMS_WorkflowStep WFS on WFS.StepID = VersionWorkflowStepID
where VH.DocumentID = T.DocumentID and WasPublishedFrom is null
order by VersionHistoryID desc
) = 2", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);

                var nonPublishedArchivedNoHistoryDocIDs = ConnectionHelper.ExecuteQuery(@"select T.DocumentID from View_CMS_Tree_Joined T Where NodeLinkedNodeID is null and NodeAliasPath like '" + SqlHelper.EscapeQuotes(path) + @"' " + siteWhere + @"
and DocumentCheckedOutVersionHistoryID is null and DocumentWorkflowStepID is not null 
and DocumentworkflowstepID in (select distinct StepID from CMS_WorkflowStep where StepType not in (100, 101))", null, QueryTypeEnum.SQLQuery).Tables[0].Rows.Cast<DataRow>().Select(x => (int)x[nameof(TreeNode.DocumentID)]);

                _Found = archivedInEditModeDocIDs.Count() + editModeNeverPublishedDocIDs.Count() + editModeNeverPublishedPublishFromDocIDs.Count() + editModePublishedWithFuturePublishFromDocIDs.Count() + editModePreviouslyPublishedDocIDs.Count() + nonPublishedArchivedNoHistoryDocIDs.Count();

                if (reportOnly)
                {

                    ReportDocument(archivedInEditModeDocIDs, lstPreviousArchivedEdit);
                    ReportDocument(editModeNeverPublishedDocIDs, lstNeverPublished);
                    ReportDocument(editModeNeverPublishedPublishFromDocIDs, lstUnpublishedWaitingPublishing);
                    ReportDocument(editModePublishedWithFuturePublishFromDocIDs, lstPublishedWithFuturePublish);
                    ReportDocument(editModePreviouslyPublishedDocIDs, lstPreviouslyPublished);
                    ReportDocument(nonPublishedArchivedNoHistoryDocIDs, lstNoVersionHistory);
                }
                else
                {
                    foreach (var docID in archivedInEditModeDocIDs)
                    {
                        HandleVersionHistory(docID, archivedInEditModeOptType, CurrentDocumentType.LatestVersion, PreviousState.WasArchived);
                    }
                    foreach (var docID in editModeNeverPublishedDocIDs)
                    {
                        HandleVersionHistory(docID, editModeNeverPublishedOptType, CurrentDocumentType.LatestVersion, PreviousState.Neither);
                    }
                    foreach (var docID in editModeNeverPublishedPublishFromDocIDs)
                    {
                        HandleVersionHistory(docID, editModeNeverPublishedPublishFromOptType, CurrentDocumentType.WaitingToPublishVersion, PreviousState.Neither);
                    }
                    foreach (var docID in editModePublishedWithFuturePublishFromDocIDs)
                    {
                        HandleVersionHistory(docID, editModePublishedWithFuturePublishFromOptType, CurrentDocumentType.WaitingToPublishVersion, PreviousState.WasPublished);
                    }
                    foreach (var docID in editModePreviouslyPublishedDocIDs)
                    {
                        HandleVersionHistory(docID, editModePreviouslyPublishedOptType, CurrentDocumentType.LatestVersion, PreviousState.WasPublished);
                    }
                    foreach (var docID in nonPublishedArchivedNoHistoryDocIDs)
                    {
                        HandleVersionHistory(docID, nonPublishedArchivedNoHistoryOptType, CurrentDocumentType.LatestVersion, PreviousState.Neither);
                    }
                }


                return $"{_Found} Items Found.  {_Archived} Archived, {_Published} Published, {_RolledBack}";
            }
        }

        public VersionHistoryOperationType GetOperationType(string value)
        {
            switch (value.ToLower())
            {
                case "rollback":
                    return VersionHistoryOperationType.RollBack;
                case "public":
                default:
                    return VersionHistoryOperationType.Publish;
                case "archive":
                    return VersionHistoryOperationType.Archive;
            }
        }

        public void DisableWorkflowAndClearHistory()
        {
            foreach (var workflow in WorkflowInfoProvider.GetWorkflows().WhereEquals(nameof(WorkflowInfo.WorkflowEnabled), false).TypedResult)
            {
                workflow.WorkflowEnabled = false;
                WorkflowInfoProvider.SetWorkflowInfo(workflow);
            }

            // Remove Pages from Workflow
            ConnectionHelper.ExecuteNonQuery(@"TRUNCATE TABLE CMS_WorkflowHistory", null, QueryTypeEnum.SQLQuery);

            // Remove Version History ref from Documents
            ConnectionHelper.ExecuteNonQuery(@"UPDATE CMS_Document SET DocumentCheckedOutVersionHistoryID = NULL ,DocumentPublishedVersionHistoryID = NULL", null, QueryTypeEnum.SQLQuery);

            // Clear History table and attachments
            ConnectionHelper.ExecuteNonQuery(@"TRUNCATE TABLE CMS_VersionAttachment", null, QueryTypeEnum.SQLQuery);
            ConnectionHelper.ExecuteNonQuery(@"DELETE FROM CMS_VersionHistory", null, QueryTypeEnum.SQLQuery);
            ConnectionHelper.ExecuteNonQuery(@"DELETE FROM CMS_AttachmentHistory", null, QueryTypeEnum.SQLQuery);

            // Clear cache and restart application
            CacheHelper.ClearCache();
            SystemHelper.RestartApplication();
        }

        private bool ReportDocument(IEnumerable<int> documentIDs, Literal list)
        {
            list.Text = "<ul>";
            foreach (var doc in DocumentHelper.GetDocuments()
                    .WhereIn(nameof(TreeNode.DocumentID), documentIDs.ToArray())
                    .LatestVersion(true)
                    .Published(false)
                    .CombineWithAnyCulture()
                    .FilterDuplicates(true)
                    .OrderBy("NodeSiteID", "DocumentNamePath")
                    .TypedResult)
            {
                list.Text += $"<li><a href=\"//{doc.Site.DomainName}/Admin/CMSAdministration.aspx?action=edit&nodeid={doc.NodeID}&culture={doc.DocumentCulture}#95a82f36-9c40-45f0-86f1-39aa44db9a77\" target=\"_blank\">GO</a> [{doc.NodeSiteName} {doc.DocumentCulture} {doc.DocumentID}] {doc.DocumentNamePath}</li>";
            }
            list.Text += "</ul>";
            return true;
        }

        public void HandleVersionHistory(int documentId, VersionHistoryOperationType operation, CurrentDocumentType version, PreviousState previousState)
        {

            TreeNode doc;
            switch (version)
            {
                case CurrentDocumentType.LatestVersion:
                default:
                    doc = DocumentHelper.GetDocuments()
                        .WhereEquals(nameof(TreeNode.DocumentID), documentId)
                        .LatestVersion(true)
                        .Published(false)
                        .CombineWithAnyCulture()
                        .FilterDuplicates(true)
                        .First();
                    break;
                case CurrentDocumentType.WaitingToPublishVersion:
                    // Get version history of the highest publish from, we will use this as the 'source' of truth.
                    var versionHistoryId = VersionHistoryInfoProvider.GetVersionHistories()
                        .WhereEquals(nameof(VersionHistoryInfo.DocumentID), documentId)
                        .WhereNotNull(nameof(VersionHistoryInfo.PublishFrom))
                        .Columns(nameof(VersionHistoryInfo.VersionHistoryID))
                        .OrderByDescending(nameof(VersionHistoryInfo.PublishFrom))
                        .TopN(1).First().VersionHistoryID;
                    doc = _Manager.GetVersion(versionHistoryId);
                    break;
            }
            try
            {
                if (operation == VersionHistoryOperationType.RollBack)
                {
                    _RolledBack++;
                    if (previousState == PreviousState.WasArchived)
                    {
                        // Undo current version
                        doc.UndoCheckOut();
                        doc.SetValue(nameof(TreeNode.DocumentPublishFrom), null);
                        doc.SetValue(nameof(TreeNode.DocumentPublishTo), null);
                        _WFManager.ArchiveDocument(doc, "Roll back and re-archiving");
                        return;
                    }
                    if (previousState == PreviousState.WasPublished)
                    {
                        // Undo current version
                        doc.UndoCheckOut();
                        doc.SetValue(nameof(TreeNode.DocumentPublishFrom), null);
                        doc.SetValue(nameof(TreeNode.DocumentPublishTo), null);
                        _WFManager.PublishDocument(doc, "Roll back and re-publishing");
                        return;
                    }
                    if (previousState == PreviousState.Neither)
                    {
                        throw new Exception("Cannot roll back a document that was neither published nor archived in the past.");
                    }
                }
                if (operation == VersionHistoryOperationType.Publish)
                {
                    _Published++;
                    try
                    {
                        _Manager.CheckOut(doc);
                    }
                    catch (WorkflowException)
                    {
                        // already checked out
                    }
                    doc.SetValue(nameof(TreeNode.DocumentPublishFrom), null);
                    doc.SetValue(nameof(TreeNode.DocumentPublishTo), null);
                    _Manager.CheckIn(doc, null, versionComment: "Publishing edited version from archived for Version History cleanup");
                    // Apply version and publish
                    _Manager.ApplyLatestVersion(doc);
                    _WFManager.PublishDocument(doc, "Publishing edited version from archived for Version History cleanup");
                    return;
                }
                if (operation == VersionHistoryOperationType.Archive)
                {
                    _Archived++;
                    try
                    {
                        _Manager.CheckOut(doc);
                    }
                    catch (WorkflowException)
                    {
                        // already checked out
                    }
                    doc.SetValue(nameof(TreeNode.DocumentPublishFrom), null);
                    doc.SetValue(nameof(TreeNode.DocumentPublishTo), null);
                    _Manager.CheckIn(doc, null, versionComment: "Archiving edited version from published for Version History cleanup");
                    // Apply version and publish
                    _Manager.ApplyLatestVersion(doc);
                    _WFManager.ArchiveDocument(doc, "Archiving edited version from published for Version History cleanup");
                    return;
                }
            }
            catch (DocumentCultureNotAllowedException ex)
            {
                // ran into this where an old document was of a culture no longer allowed on a site. Delete
                doc.Delete();
            }
        }
    }

}

