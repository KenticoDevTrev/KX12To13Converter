using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessWidgetSectionZoneEventArgs : CMSEventArgs
    {
        public PortalToMVCProcessWidgetSectionZoneEventArgs() { }

        public PortalToMVCProcessWidgetSectionZoneEventArgs(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCProcessDocumentPrimaryEventArgs,
            PortalWidgetSection portalWidgetSection,
            SectionConfiguration sectionConfiguration,
            int zoneIndex)
        {
            PrimaryEventArgs = portalToMVCProcessDocumentPrimaryEventArgs;
            PortalSectionWidget = portalWidgetSection;
            PageBuilderSection = sectionConfiguration;
            ZoneIndex = zoneIndex;
        }

        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;

        /// <summary>
        /// The Source Portal Engine Layout Widget
        /// </summary>
        public readonly PortalWidgetSection PortalSectionWidget;

        /// <summary>
        /// The Page Builder Section Containing this zone
        /// </summary>
        public readonly SectionConfiguration PageBuilderSection;

        /// <summary>
        /// The index of the zone
        /// </summary>
        public readonly int ZoneIndex;

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's Name to IGNORE to not processes
        /// </summary>
        public ZoneConfiguration PageBuilderZoneConfig { get; set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool SectionZoneHandled { get; set; } = false;
    }
}
