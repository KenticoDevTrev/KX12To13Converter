using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;
using KX12To13Converter.Base.PageOperations;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.UpgradeScripts
{
    public partial class RemoveClasses : CMSPage
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var pageTypes = PreUpgrade2RemoveClasses.GetPageTypes();

            foreach (var pageType in pageTypes)
            {
                // Check to see if any Database references found
                WriteClassEntry(pageType);
            }

            var usedPageTypes = PreUpgrade2RemoveClasses.GetUsedPageTypes();

            foreach (var pageType in usedPageTypes)
            {
                // Check to see if any Database references found
                WriteClassEntryForUsed(pageType);
            }

        }

        public void WriteClassEntry(DataClassInfo classObj)
        {
            cbxClasses.Items.Add(new ListItem(PreUpgrade2RemoveClasses.GetEntryValue(classObj), classObj.ClassID.ToString()));
        }

        public void WriteClassEntryForUsed(DataClassInfo classObj)
        {
            cbxUsedClasses.Items.Add(new ListItem(PreUpgrade2RemoveClasses.GetEntryValueForUsed(classObj), classObj.ClassID.ToString()));
        }

       


        protected void btnDeleteClasses_Click(object sender, EventArgs e)
        {
            var selected = cbxClasses.Items.Cast<ListItem>().Where(x => x.Selected);
            var classIds = selected.Select(x => ValidationHelper.GetInteger(x.Value, 0));
            PreUpgrade2RemoveClasses.DeleteClasses(classIds);

            foreach (var selectedItem in selected)
            {
                try
                {
                    cbxClasses.Items.Remove(selectedItem);
                }
                catch { }
            }
            ltrResult.Text = $"{selected.Count()} Classes Removed.";
        }

        protected void btnDeletePagesAndClasses_Click(object sender, EventArgs e)
        {
            var selected = cbxUsedClasses.Items.Cast<ListItem>().Where(x => x.Selected);
            var classIds = selected.Select(x => ValidationHelper.GetInteger(x.Value, 0));
            // Delete all pages
            PreUpgrade2RemoveClasses.DeletePagesClasses(classIds);

            // remove entries
            try
            {
                foreach (var selectedItem in selected)
                {
                    cbxUsedClasses.Items.Remove(selectedItem);
                }
            }
            catch { }
            ltrResult.Text = $"{selected.Count()} Classes Removed.";
        }
    }

}