using KX12To13Converter.Base.Classes.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.Base.Classes.PortalEngineToPageBuilder
{
    public class PageBuilderData
    {
        public PageTemplateConfiguration TemplateConfiguration { get; set; } = new PageTemplateConfiguration();
        public EditableAreasConfiguration ZoneConfiguration { get; set; } = new EditableAreasConfiguration();
    }
}
