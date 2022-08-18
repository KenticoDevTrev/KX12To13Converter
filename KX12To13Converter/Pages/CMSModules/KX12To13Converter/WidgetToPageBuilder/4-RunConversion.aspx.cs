using CMS.DataEngine;
using CMS.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Formatting = Newtonsoft.Json.Formatting;
using TreeNode = CMS.DocumentEngine.TreeNode;
using CMS.SiteProvider;
using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder;
using KX12To13Converter.Base.PageOperations;
using CMS.UIControls;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder
{
    public partial class RunConversion : CMSPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (SessionHelper.GetValue("tbxTemplateConfigJson") != null)
            {
                tbxTemplateConfigJson.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxTemplateConfigJson"), string.Empty);
            }
            if (SessionHelper.GetValue("tbxSectionConfigJson") != null)
            {
                tbxSectionConfigJson.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxSectionConfigJson"), string.Empty);
            }
            if (SessionHelper.GetValue("tbxDefaultSectionConfigJson") != null)
            {
                tbxDefaultSectionConfigJson.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxDefaultSectionConfigJson"), string.Empty);
            }
            if (SessionHelper.GetValue("tbxWidgetConfiguration") != null)
            {
                tbxWidgetConfiguration.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxWidgetConfiguration"), string.Empty);
            }
            if (SessionHelper.GetValue("tbxPreviewPath") != null)
            {
                tbxPreviewPath.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxPreviewPath"), string.Empty);
            }
            if (SessionHelper.GetValue("tbxProcessPath") != null)
            {
                tbxProcessPath.Text = ValidationHelper.GetString(SessionHelper.GetValue("tbxProcessPath"), string.Empty);
            }
            if (SessionHelper.GetValue("ddlConfigMode") != null)
            {
                ddlConfigMode.SelectedValue = ValidationHelper.GetString(SessionHelper.GetValue("ddlConfigMode"), string.Empty);
            }
            if (SessionHelper.GetValue("ddlMode") != null)
            {
                ddlMode.SelectedValue = ValidationHelper.GetString(SessionHelper.GetValue("ddlMode"), string.Empty);
            }
        }

        protected void btnConvert_Click(object sender, EventArgs e)
        {
            // save data in session
            SessionHelper.SetValue("ddlConfigMode", ddlConfigMode.SelectedValue);
            SessionHelper.SetValue("ddlMode", ddlMode.SelectedValue);

            List<TemplateConfiguration> templateConfiguration;
            List<ConverterSectionConfiguration> sectionConfiguration;
            PageBuilderSection defaultSectionConfiguration;
            List<ConverterWidgetConfiguration> widgetConfiguration;

            if (ddlConfigMode.SelectedValue.Equals("Manual", StringComparison.OrdinalIgnoreCase))
            {
                SessionHelper.SetValue("tbxTemplateConfigJson", tbxTemplateConfigJson.Text);
                SessionHelper.SetValue("tbxSectionConfigJson", tbxSectionConfigJson.Text);
                SessionHelper.SetValue("tbxDefaultSectionConfigJson", tbxDefaultSectionConfigJson.Text);
                SessionHelper.SetValue("tbxWidgetConfiguration", tbxWidgetConfiguration.Text);
                SessionHelper.SetValue("tbxPreviewPath", tbxPreviewPath.Value);

                templateConfiguration = JsonConvert.DeserializeObject<List<TemplateConfiguration>>(tbxTemplateConfigJson.Text);
                sectionConfiguration = JsonConvert.DeserializeObject<List<ConverterSectionConfiguration>>(tbxSectionConfigJson.Text);
                defaultSectionConfiguration = JsonConvert.DeserializeObject<PageBuilderSection>(tbxDefaultSectionConfigJson.Text);
                widgetConfiguration = JsonConvert.DeserializeObject<List<ConverterWidgetConfiguration>>(tbxWidgetConfiguration.Text);
            }
            else
            {
                SessionHelper.SetValue("tbxProcessPath", tbxProcessPath.Value);
                templateConfiguration = JsonConvert.DeserializeObject<List<TemplateConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterTemplateConfigJson", SiteContext.CurrentSiteID));
                sectionConfiguration = JsonConvert.DeserializeObject<List<ConverterSectionConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterSectionConfigJson", SiteContext.CurrentSiteID));
                defaultSectionConfiguration = JsonConvert.DeserializeObject<PageBuilderSection>(SettingsKeyInfoProvider.GetValue("ConverterDefaultSectionConfigJson", SiteContext.CurrentSiteID));
                widgetConfiguration = JsonConvert.DeserializeObject<List<ConverterWidgetConfiguration>>(SettingsKeyInfoProvider.GetValue("ConverterWidgetConfigJson", SiteContext.CurrentSiteID));
            }

            var converter = new PortalEngineToMVCConverter(templateConfiguration, sectionConfiguration, defaultSectionConfiguration, widgetConfiguration, new ProcessDocumentSettings());

            switch (ddlMode.SelectedValue.ToLower())
            {
                case "preview":
                    var previewDoc = WidgetConverterMethods.GetDocumentByPath(tbxPreviewPath.Value.ToString());
                    converter.ProcessesDocuments(new List<TreeNode>() { previewDoc }, HandlePreviewDocument);
                    break;
                case "processandsave":
                case "processandsend":
                    var documents = WidgetConverterMethods.GetDocumentsByPath(tbxProcessPath.Value.ToString());
                    if(ddlMode.SelectedValue.ToLower().Equals("processandsave", StringComparison.OrdinalIgnoreCase))
                    {
                        converter.ProcessesDocuments(documents, WidgetConverterMethods.HandleProcessAndSaveDocument);
                    } else
                    {
                        converter.ProcessesDocuments(documents, WidgetConverterMethods.HandleProcessAndSendDocument);
                    }
                    break;
            }
        }

        private bool HandlePreviewDocument(TreeNode document, PortalToMVCProcessDocumentPrimaryEventArgs results)
        {
            tbxJsonResultTemplate.Text = (JsonConvert.SerializeObject(results.PageBuilderData.TemplateConfiguration, Formatting.Indented).Trim());
            tbxJsonResultWidgets.Text = (JsonConvert.SerializeObject(results.PageBuilderData.ZoneConfiguration, Formatting.Indented).Trim());
            return true;
        }

    }
}