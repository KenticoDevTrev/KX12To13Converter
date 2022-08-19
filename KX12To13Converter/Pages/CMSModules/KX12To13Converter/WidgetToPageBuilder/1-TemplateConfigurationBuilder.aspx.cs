using CMS.Core;
using CMS.UIControls;
using KX12To13Converter.Interfaces;
using Newtonsoft.Json;
using System;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder
{
    public partial class TemplateConfigurationBuilder : CMSPage
    {

        public TemplateConfigurationBuilder()
        {
            TemplateConfigurationBuilderMethods = Service.Resolve<ITemplateConfigurationBuilderMethods>();
        }

        public ITemplateConfigurationBuilderMethods TemplateConfigurationBuilderMethods { get; }

        protected void btnGenerateTemplates_Click(object sender, EventArgs e)
        {
            // Get list of templates to convert.
            
            var includedTemplateIds = TemplateConfigurationBuilderMethods.GetTemplateIdsByCodeName(tbxIncluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            var excludedTemplateIds = TemplateConfigurationBuilderMethods.GetTemplateIdsByCodeName(tbxExcluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            // get all current used templates
            var currentTemplateIds = TemplateConfigurationBuilderMethods.GetCurrentTemplateIds();

            var templateConfigs = TemplateConfigurationBuilderMethods.GetTemplateConfigurations(currentTemplateIds, includedTemplateIds, excludedTemplateIds);
            tbxJsonStructure.Text = JsonConvert.SerializeObject(templateConfigs, Formatting.Indented);
        }
    }
}