using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessEditableAreaEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessEditableAreaEventArgs() { }

        public PortalToMVCProcessEditableAreaEventArgs(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCProcessDocumentPrimaryEventArgs,
            PortalWebpartZone portalWebpartZone,
            TemplateConfiguration templateConfiguration)
        {
            PrimaryEventArgs = portalToMVCProcessDocumentPrimaryEventArgs;
            PortalEditableArea = portalWebpartZone;
            TemplateConfiguration = templateConfiguration;
        }
        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;
        /// <summary>
        /// Source Portal Engine Webpart Zone
        /// </summary>
        public readonly PortalWebpartZone PortalEditableArea;

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Identifier to null / empty / IGNORE to not processes
        /// </summary>
        public EditableAreaConfiguration EditableAreaConfiguration { get; set; }

        /// <summary>
        /// The current Portal Engine Template Configuration. (May be null if no configuration provided)
        /// Can find the webpart zone info through TemplateConfiguration.Zones.Where(x => x.PE_WebpartZoneName.Equals(peZoneID, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()
        /// </summary>
        public readonly TemplateConfiguration TemplateConfiguration;

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool ZoneHandled { get; set; } = false;
        
    }
}
