using CMS.UIControls;
using KX12To13Converter.Interfaces;
using Newtonsoft.Json;
using System;

namespace KX12To13Converter.Pages.CMSModules.KX12To13Converter.WidgetToPageBuilder
{
    public partial class WidgetConfigurationBuilder : CMSPage
    {
        public WidgetConfigurationBuilder()
        {
            WidgetConfigurationBuilderMethods = CMS.Core.Service.Resolve<IWidgetConfigurationBuilderMethods>();
        }

        public IWidgetConfigurationBuilderMethods WidgetConfigurationBuilderMethods { get; }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            var includedIds = WidgetConfigurationBuilderMethods.GetWidgetIdsByWidgetName(tbxIncluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
            var excludedIds = WidgetConfigurationBuilderMethods.GetWidgetIdsByWidgetName(tbxExcluded.Text.Split("\n\r;,|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

            // get all current used templates
            var widgetConfigurations = WidgetConfigurationBuilderMethods.GetWidgetConfigurations(includedIds, excludedIds);

            tbxJsonStructure.Text = JsonConvert.SerializeObject(widgetConfigurations, Formatting.Indented);
        }
    }
}