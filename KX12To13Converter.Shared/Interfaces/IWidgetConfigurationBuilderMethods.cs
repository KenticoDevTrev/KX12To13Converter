using KX12To13Converter.PortalEngineToPageBuilder;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KX12To13Converter.Interfaces
{
    public interface IWidgetConfigurationBuilderMethods
    {
        IEnumerable<int> GetWidgetIdsByWidgetName(IEnumerable<string> widgetNames);
        List<ConverterWidgetConfiguration> GetWidgetConfigurations(IEnumerable<int> includedWidgetIds, IEnumerable<int> excludedWidgetIds);
    }
}
