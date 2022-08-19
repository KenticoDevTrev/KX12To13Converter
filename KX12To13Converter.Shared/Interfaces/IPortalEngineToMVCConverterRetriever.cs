using KX12To13Converter.PortalEngineToPageBuilder;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface IPortalEngineToMVCConverterRetriever
    {
        IPortalEngineToMVCConverter GetConverter(List<TemplateConfiguration> templateConfigurations,
            List<ConverterSectionConfiguration> sectionConfigurations,
            PageBuilderSection defaultSectionConfiguration,
            List<ConverterWidgetConfiguration> widgetConfigurations);
    }
}
