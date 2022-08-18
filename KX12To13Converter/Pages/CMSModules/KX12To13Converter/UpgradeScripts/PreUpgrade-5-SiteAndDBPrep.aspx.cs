using CMS.Helpers;
using CMS.UIControls;
using KX12To13Converter.Base.PageOperations;
using System;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts
{
    public partial class SiteAndDBPrep : CMSPage
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected void btnSetSitesToContentOnly_Click(object sender, EventArgs e)
        {
            ltrResult.Text = String.Empty;
            PreUpgrade5SiteAndDBPrep.SetSitesToContentOnly();
            ltrResult.Text = "<div class='alert alert-info'>Sites set to Content Only.  Additional configuration on each site may be required (setting admin/presentation urls)</div>";
        }

        protected void btnRunPreUpgradeFixes_Click(object sender, EventArgs e)
        {
            ltrResult.Text = "";
            string results = "";
            PreUpgrade5SiteAndDBPrep.RunPreUpgradeFixes(ref results);
            if (string.IsNullOrWhiteSpace(results))
            {
                ltrResult.Text += "<div class='alert alert-info'>Operations Completed</div>";
            } else
            {
                ltrResult.Text = results;
            }
        }
        protected void btnSetCompatabilityLevel_Click(object sender, EventArgs e)
        {
            ltrResult.Text = String.Empty;
            string results = "";
            PreUpgrade5SiteAndDBPrep.SetCompatibilityLevelForDatabase(txtDatabaseName.Text, ref results);
            CacheHelper.ClearCache();
            SystemHelper.RestartApplication();
            if (string.IsNullOrWhiteSpace(results))
            {
                ltrResult.Text += "<div class='alert alert-info'>Operations Completed</div>";
            } else
            {
                ltrResult.Text = results;
            }
        }

    }

}