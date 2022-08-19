using CMS.Core;
using CMS.UIControls;
using KX12To13Converter.Interfaces;
using Newtonsoft.Json;
using System;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder
{
    public partial class SectionConfigurationBuilder : CMSPage
    {

        public SectionConfigurationBuilder()
        {
            SectionConfigurationBuilderMethods = Service.Resolve<ISectionConfigurationBuilderMethods>();
        }

        public ISectionConfigurationBuilderMethods SectionConfigurationBuilderMethods { get; }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            var includedIds = SectionConfigurationBuilderMethods.GetSectionWidgetIdsByWidgetName(tbxIncluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            var excludedIds = SectionConfigurationBuilderMethods.GetSectionWidgetIdsByWidgetName(tbxExcluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            var currentWidgetIds = SectionConfigurationBuilderMethods.GetCurrentSectionWidgets();

            var sectionConfigurations = SectionConfigurationBuilderMethods.GetSectionConfigurations(currentWidgetIds, includedIds, excludedIds);
            tbxJsonStructure.Text = JsonConvert.SerializeObject(sectionConfigurations, Formatting.Indented);

            var defaultSectionConfiguration = SectionConfigurationBuilderMethods.GetDefaultSectionConfiguration();
            tbxDefaultSectionJson.Text = JsonConvert.SerializeObject(defaultSectionConfiguration, Formatting.Indented);
        }
    }
}