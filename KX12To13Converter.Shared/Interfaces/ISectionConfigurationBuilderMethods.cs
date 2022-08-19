using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface ISectionConfigurationBuilderMethods
    {
        IEnumerable<int> GetSectionWidgetIdsByWidgetName(IEnumerable<string> widgetNames);
        IEnumerable<int> GetCurrentSectionWidgets();
        List<ConverterSectionConfiguration> GetSectionConfigurations(IEnumerable<int> currentWidgetIds, IEnumerable<int> includedWidgetIds, IEnumerable<int> excludedWidgetIds);
        PageBuilderSection GetDefaultSectionConfiguration();
    }
}
