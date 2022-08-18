using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.WorkflowEngine;
using KX12To13Converter.Base.PageOperations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;
using TreeNode = CMS.DocumentEngine.TreeNode;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts
{
    public partial class VersioningWorkflow : CMSPage
    {
        public VersioningWorkflow()
        {
           
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var sites = SiteInfoProvider.GetSites()
                .Select(x => new Tuple<int, string>(x.SiteID, x.SiteName))
                .Union(new List<Tuple<int, string>>() { new Tuple<int, string>(-1, "All Sites") })
                .OrderBy(x => x.Item1 == -1 ? 0 : 1)
                .ThenBy(x => x.Item2)
                .Select(x => new ListItem(x.Item2, x.Item1.ToString()));

            ddlSite.Items.Clear();
            ddlSite.Items.AddRange(sites.ToArray());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lstNeverPublished.Text = String.Empty;
            lstNoVersionHistory.Text = String.Empty;
            lstPreviousArchivedEdit.Text = String.Empty;
            lstPreviouslyPublished.Text = String.Empty;
            lstPublishedWithFuturePublish.Text = String.Empty;
            lstUnpublishedWaitingPublishing.Text = String.Empty;
        }

        protected void btnPublishAll_Click(object sender, EventArgs e)
        {
            PublishCheckInArchive();
        }

        public void PublishCheckInArchive(bool reportOnly = false)
        {
            string path = !string.IsNullOrWhiteSpace(txtPath.Value?.ToString() ?? "") ? txtPath.Text : "/%";
            path = path.EndsWith("%") ? path : path + "%";
            int siteID = ValidationHelper.GetInteger(ddlSite.SelectedValue, -1);
            var upgrader = new PreUpgrade1VersioningWorkflow();

            var archivedInEditModeOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlPreviousArchivedEdit.SelectedValue);
            var editModeNeverPublishedOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlNeverPublished.SelectedValue);
            var editModeNeverPublishedPublishFromOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlUnpublishedWaitingPublishing.SelectedValue);
            var editModePublishedWithFuturePublishFromOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlPublishedWithFuturePublish.SelectedValue);
            var editModePreviouslyPublishedOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlPreviouslyPublished.SelectedValue);
            var nonPublishedArchivedNoHistoryOptType = PreUpgrade1VersioningWorkflow.GetOperationType(ddlNoVersionHistory.SelectedValue);


            upgrader.PublishCheckInArchive(path, siteID, 
                archivedInEditModeOptType, 
                editModeNeverPublishedOptType,
                editModeNeverPublishedPublishFromOptType,
                editModePublishedWithFuturePublishFromOptType,
                editModePreviouslyPublishedOptType,
                nonPublishedArchivedNoHistoryOptType,
                lstPreviousArchivedEdit,
                lstNeverPublished,
                lstUnpublishedWaitingPublishing,
                lstPublishedWithFuturePublish,
                lstPreviouslyPublished,
                lstNoVersionHistory,
                reportOnly);

            
        }

        protected void btnPublishAllReport_Click(object sender, EventArgs e)
        {
            PublishCheckInArchive(true);
        }

        protected void btnDisableWorkflowClearHistory_Click(object sender, EventArgs e)
        {
            PreUpgrade1VersioningWorkflow.DisableWorkflowAndClearHistory();
        }
    }

   
}