using KX12To13Converter.PortalEngineToPageBuilder;
using System.Collections.Generic;

namespace KX12To13Converter.Interfaces
{
    public interface ITemplateConfigurationBuilderMethods
    {
        IEnumerable<int> GetTemplateIdsByCodeName(IEnumerable<string> templateCodeNames);
        IEnumerable<int> GetCurrentTemplateIds();
        List<TemplateConfiguration> GetTemplateConfigurations(IEnumerable<int> currentTemplateIds, IEnumerable<int> includedTemplateIds, IEnumerable<int> excludedTemplateIds);
    }
}
