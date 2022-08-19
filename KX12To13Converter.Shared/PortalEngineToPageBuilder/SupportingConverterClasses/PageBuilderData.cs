namespace KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses
{
    public class PageBuilderData
    {
        /// <summary>
        /// The Page Builder Template Configuration
        /// </summary>
        public PageTemplateConfiguration TemplateConfiguration { get; set; } = new PageTemplateConfiguration();

        /// <summary>
        /// The Page Builder Editable Areas that sections and their widgets will go in
        /// </summary>
        public EditableAreasConfiguration ZoneConfiguration { get; set; } = new EditableAreasConfiguration();
    }
}
