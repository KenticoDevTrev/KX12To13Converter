using CMS.DataEngine;
using CMS.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Formatting = Newtonsoft.Json.Formatting;
using TreeNode = CMS.DocumentEngine.TreeNode;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.DocumentEngine;
using System.Linq;
using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder.EventArgs;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder
{
    public partial class RunConversion : CMSPage
    {
        public IPortalEngineToMVCConverterRetriever PortalEngineToMVCConverterRetriever { get; }
        public IConversionProcessingMethods ConversionProcessingMethods { get; }
        public IRunConversionMethods RunConversionMethods { get; }

        public RunConversion()
        {
            PortalEngineToMVCConverterRetriever = CMS.Core.Service.Resolve<IPortalEngineToMVCConverterRetriever>();
            ConversionProcessingMethods = CMS.Core.Service.Resolve<IConversionProcessingMethods>();
            RunConversionMethods = CMS.Core.Service.Resolve<IRunConversionMethods>();
        }

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

            var converter = PortalEngineToMVCConverterRetriever.GetConverter(templateConfiguration, sectionConfiguration, defaultSectionConfiguration, widgetConfiguration);

            if (ddlMode.SelectedValue.Equals("preview", StringComparison.OrdinalIgnoreCase))
            {
                var previewDoc = RunConversionMethods.GetDocumentByPath(tbxPreviewPath.Value.ToString());
                if(previewDoc != null) { 
                    converter.ProcessesDocuments(new List<TreeNode>() { previewDoc }, HandlePreviewDocument);
                } else
                {
                    tbxJsonResultTemplate.Text = $"NO PAGE FOUND AT {tbxPreviewPath.Value}";
                    tbxJsonResultWidgets.Text = $"NO PAGE FOUND AT {tbxPreviewPath.Value}";
                }
            }
            else
            {
                var documents = RunConversionMethods.GetDocumentsByPath(tbxProcessPath.Value.ToString());
                switch (ddlMode.SelectedValue.ToLower())
                {
                    case "process":
                        converter.ProcessesDocuments(documents, ConversionProcessingMethods.HandleProcessOnly);
                        break;
                    case "processandsave":
                        converter.ProcessesDocuments(documents, ConversionProcessingMethods.HandleProcessAndSaveDocument);
                        break;
                    case "processandsend":
                        converter.ProcessesDocuments(documents, ConversionProcessingMethods.HandleProcessAndSendDocument);
                        break;
                }
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