using KX12To13Converter.Interfaces;
using KX12To13Converter.PortalEngineToPageBuilder;
using System.Collections.Generic;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PortalEngineToMVCConverterRetriever : IPortalEngineToMVCConverterRetriever
    {
        public IPortalEngineToMVCConverter GetConverter(List<TemplateConfiguration> templateConfigurations, List<ConverterSectionConfiguration> sectionConfigurations, PageBuilderSection defaultSectionConfiguration, List<ConverterWidgetConfiguration> widgetConfigurations)
        {
            return new PortalEngineToMVCConverter(templateConfigurations, sectionConfigurations, defaultSectionConfiguration, widgetConfigurations);
        }
    }
}
