using CMS.Base;
using KX12To13Converter.PortalEngineToPageBuilder.SupportingConverterClasses;
using System.Collections.ObjectModel;

namespace KX12To13Converter.PortalEngineToPageBuilder.EventArgs
{
    public class PortalToMVCProcessWidgetSectionEventArgs : CMSEventArgs
    {

        public PortalToMVCProcessWidgetSectionEventArgs() { }

        public PortalToMVCProcessWidgetSectionEventArgs(PortalToMVCProcessDocumentPrimaryEventArgs portalToMVCProcessDocumentPrimaryEventArgs,
            PortalWidgetSection portalWidgetSection,
            ReadOnlyCollection<ConverterSectionConfiguration> converterSectionConfigurations,
            PageBuilderSection defaultSection)
        {
            PrimaryEventArgs = portalToMVCProcessDocumentPrimaryEventArgs;
            PortalEngineWidgetSection = portalWidgetSection;
            SectionConfigurations = converterSectionConfigurations;
            DefaultSectionConfiguration = defaultSection;

        }
        public readonly PortalToMVCProcessDocumentPrimaryEventArgs PrimaryEventArgs;

        /// <summary>
        /// Source Portal Engine Widget Section
        /// </summary>
        public readonly PortalWidgetSection PortalEngineWidgetSection;

        /// <summary>
        /// Configurations Available, for reference only.  
        /// Can find proper config through:  
        /// PortalEngineWidgetSection.IsDefault ? null : SectionConfigurations.Where(x => (x.PE_WidgetZoneSection?.SectionWidgetCodeName ?? String.Empty).Equals(PortalEngineWidgetSection.IsDefault ? Guid.NewGuid().ToString() : PortalEngineWidgetSection.SectionWidget.WebPartType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        /// </summary>
        public readonly ReadOnlyCollection<ConverterSectionConfiguration> SectionConfigurations;

        /// <summary>
        /// The Default Section Configuration (if given), used for if there are no sections
        /// </summary>
        public readonly PageBuilderSection DefaultSectionConfiguration;

        /// <summary>
        /// Edit this to adjust the Page Builder output.  Can set it's TypeIdentifier to null / empty / IGNORE to not processes
        /// </summary>
        public SectionConfiguration PageBuilderSection { get; set; }

        /// <summary>
        /// Set to true to bypass default logic
        /// </summary>
        public bool SectionHandled { get; set; } = false;
    }
}
